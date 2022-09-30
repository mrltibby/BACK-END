using Firebase.Auth;
using Firebase.Storage;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace pdfapis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BionicConverter : ControllerBase
    {
        static readonly HttpClient client = new HttpClient();
        // GET: api/<BionicConverter>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<BionicConverter>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        // POST api/<BionicConverter>
        [HttpPost]
        public async Task<IEnumerable<string>> Post(string url)
        {
          /*  return new string[] { "frombionic" };*/
           /* HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load(url);
            var body = document.DocumentNode.SelectNodes("//body").First().InnerText;
            Console.WriteLine(body.Split(" ").Length);
            Console.WriteLine(body);
            var words = Regex.Replace(body, @"\t|\n|\r", " ").Split(" ");*/

            const string path = @".\result.html";


            string convertedBionic = @".\bionic.html";

           


            int start = 0;
            StringBuilder c = new StringBuilder();
            foreach (var line in System.IO.File.ReadLines(path))
            {
                String result = Regex.Replace(line, @"<[^>]*>", String.Empty);
                //String result = Regex.Replace(line, @"<style[^<] *</ style\s*>", String.Empty);
               
           
                    // Use stream
                    if (line.Contains("body"))
                    {
                        start += 1;
                    }


                    if (start == 1 && !line.Equals("") && start != 2 && result.Length != 0)
                    {
                        //

                        string left = line.Replace("'", @"'").Substring(0, line.Length - (result.Length + (line.Contains("</a>") ? 11 : 7)));
                        string right = line.Substring(left.Length + result.Length);

                        string replace = left + Tobionic(result) + right;

                    /*if (result.Contains("However, until 2006"))
                    {

                        Console.WriteLine(left);
                        Console.WriteLine(Tobionic(result));
                        Console.WriteLine(right);
                    }*/


                    // System.IO.File.AppendAllText(convertedBionic, replace + Environment.NewLine);
                    /* var s = new StreamWriter(convertedBionic, append: true);
                     s.Close();*/
                    c.Append(replace + Environment.NewLine);
                    continue;
                    }
                //System.IO.File.AppendAllText(convertedBionic, line + Environment.NewLine);
                /*var v = new StreamWriter(convertedBionic, append: true);
                v.Close();*/
                c.Append(line + Environment.NewLine);


            }

            /* Console.WriteLine(lines.Length);*/



            /* StreamWriter sw = new StreamWriter(path);
             sw.WriteLine(c.ToString());
             sw.Close();*/
           
            FileW p = new FileW();
            string dl = await p.WriteAsync(c.ToString());
            Console.WriteLine(dl);
            return new string[] { dl};
        
            //



        }

       


        public static int GetBoldLength(int length)
        {
            var lengthToCal = length;
            if (length <= 0)
                return lengthToCal;

            if (length == 3)
                lengthToCal = 2;
            else
                lengthToCal = length + 1;
            return lengthToCal / 2;
        }


        public static string Tobionic(string line)
        {
            StringWriter myWriter = new StringWriter();

            // Decode the encoded string.
          
            string result = "";
            foreach (var word in line.Split(" "))
            {
                if (word.Contains("&"))
                {
                    HttpUtility.HtmlDecode(word, myWriter);
                    string myDecodedString = myWriter.ToString();
                    Console.WriteLine(GetBoldLength(myDecodedString.Length));
                    //result += word.Insert(GetBoldLength(myDecodedString.Length), "</b>").Insert(0, "<b>") + " ";
                    continue;
                }
                result += word.Insert(GetBoldLength(word.Length), "</b>").Insert(0, "<b>")+" ";
               
            }
            return result;
        }

        // PUT api/<BionicConverter>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<BionicConverter>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

    }
}
