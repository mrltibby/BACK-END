

using Firebase.Auth;
using Firebase.Storage;

namespace pdfapis.Controllers
{
    public class FileW
    {
        public async Task<string> WriteAsync(string data)
        {
            string guid = Guid.NewGuid().ToString();
            string convertedBionic = @".\"+guid+".html";
            using (var sw = new StreamWriter(convertedBionic))
            {
                await sw.WriteAsync(data);
                sw.Close();
                return await FirebaseUploadAsync(convertedBionic) + "";

            }
          
            
        }

        protected static  bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
        static async Task<string> FirebaseUploadAsync(string path)
        {
            Console.WriteLine("UPLOADING");
            FileStream stream;
            FileInfo f = new FileInfo(path);
            while (true)
            {
                var x = !IsFileLocked(f);
                    Console.WriteLine(x);
                if (x)
                {
                   stream = System.IO.File.OpenRead(path);
                   break;
                }
            }
            Console.WriteLine("UPLOADING");



            //CONVERT HTML TO PDF

            //authentication
            var auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCRDMrr9fhO9hH1_OFmlv-BOzfsmy8D3Sg"));
                var a = await auth.SignInWithEmailAndPasswordAsync("mrltibs@gmail.com", "Justfortest#123");

                // Constructr FirebaseStorage, path to where you want to upload the file and Put it there
                var task = new FirebaseStorage(
                    "bionicwebapp.appspot.com",

                     new FirebaseStorageOptions
                     {
                         AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                         ThrowOnCancel = true,
                     })
                    .Child("bionic")
                    .Child("" + Guid.NewGuid() + ".html")
                    .PutAsync(stream);

                // Track progress of the upload
                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress FROM BIONIC CONTROLLER: {e.Percentage} %");

                // await the task to wait until upload completes and get the download url
                var downloadUrl = await task;
            Console.WriteLine(downloadUrl);
                return downloadUrl;
            }

      
    }
}
