using Microsoft.Owin;
using Owin;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Configuration;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;
using System.Security.Cryptography;
using System.Security;

[assembly: OwinStartup(typeof(MonoWebApp.Startup))]
namespace MonoWebApp
{
    public class DotvvmStartup : IDotvvmStartup
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.Debug = true;
            config.RouteTable.Add("Calculator", "Calculator", "Views/Calculator.dothtml", new { });
        }
    }

    public class AesDataProtector : IDataProtector
    {
        private readonly byte[] key;

        public AesDataProtector(string key)
        {
            using (var sha1 = new SHA256Managed())
            {
                this.key = sha1.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
        }

        public byte[] Protect(byte[] userData)
        {
            byte[] dataHash;
            using (var sha = new SHA256Managed())
            {
                dataHash = sha.ComputeHash(userData);
            }

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.GenerateIV();

                using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, 16);

                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var bwEncrypt = new BinaryWriter(csEncrypt))
                    {
                        bwEncrypt.Write(dataHash);
                        bwEncrypt.Write(userData.Length);
                        bwEncrypt.Write(userData);
                    }
                    var protectedData = msEncrypt.ToArray();
                    return protectedData;
                }
            }
        }
        public byte[] Unprotect(byte[] protectedData)
        {
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;

                using (var msDecrypt = new MemoryStream(protectedData))
                {
                    byte[] iv = new byte[16];
                    msDecrypt.Read(iv, 0, 16);

                    aesAlg.IV = iv;

                    using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var brDecrypt = new BinaryReader(csDecrypt))
                    {
                        var signature = brDecrypt.ReadBytes(32);
                        var len = brDecrypt.ReadInt32();
                        var data = brDecrypt.ReadBytes(len);

                        byte[] dataHash;
                        using (var sha = new SHA256Managed())
                        {
                            dataHash = sha.ComputeHash(data);
                        }

                        if (!dataHash.SequenceEqual(signature))
                        {
                            throw new SecurityException("Signature does not match the computed hash");
                        }

                        return data;
                    }
                }
            }
        }
    }

    public class AesDataProtectionProvider : IDataProtectionProvider
    {
        private string appName;

        public AesDataProtectionProvider() : this(Guid.NewGuid().ToString())
        {
        }

        public AesDataProtectionProvider(string appName)
        {
            if (appName == null)
            {
                throw new ArgumentNullException("appName");
            }
            this.appName = appName;
        }

        public IDataProtector Create(params string[] purposes)
        {
            return new AesDataProtector(appName + ":" + string.Join(",", purposes));
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.SetDataProtectionProvider(new AesDataProtectionProvider());

            // DotVVM initialization
            var applicationPhysicalPath = Directory.GetCurrentDirectory();
            var dotvvmConfiguration = appBuilder.UseDotVVM<DotvvmStartup>(applicationPhysicalPath);

            appBuilder.UseFileServer(new FileServerOptions()
            {
                RequestPath = new PathString("/Scripts"),
                FileSystem = new PhysicalFileSystem(@"./Scripts"),
            });

            appBuilder.UseFileServer(new FileServerOptions()
            {
                RequestPath = new PathString("/fonts"),
                FileSystem = new PhysicalFileSystem(@"./fonts"),
            });

            appBuilder.UseFileServer(new FileServerOptions()
            {
                RequestPath = new PathString("/Content"),
                FileSystem = new PhysicalFileSystem(@"./Content"),
            });

            appBuilder.Map("/UpdateValue", app2 =>
            {
                app2.Use((context, next) =>
                {
                    Program.updateWait.WaitOne();

                    context.Response.ContentType = "text/plain";
                    context.Response.Write(Program.updateValue);

                    return Task.FromResult(0);
                });
            });

            appBuilder.Run(context =>
            {
                try
                {
                    if (context.Request.Path.Value.Equals("/favicon.ico", StringComparison.InvariantCultureIgnoreCase))
                    {
                    }

                    return Task.FromResult(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in /: " + ex.Message);
                    return Task.FromResult(0);
                }
            });
        }
    }
}
