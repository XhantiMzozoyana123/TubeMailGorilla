using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TubeMailGorilla.Dtos;
using TubeMailGorilla.Entities;

namespace TubeMailGorilla.Api
{
    public static class EmailerApi
    {
        public static bool PageViewSearch(SearchDto searchViewModels)
        {
            ApplicationDbContext appDbContext = new ApplicationDbContext();

            try
            {
                var url = (appDbContext.Settings.First().ApiUrl) + ("api/extract/pageview");

                var request = WebRequest.Create(url);
                request.Method = "POST";

                var json = JsonConvert.SerializeObject(searchViewModels);
                byte[] byteArray = Encoding.UTF8.GetBytes(json);

                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;

                var reqStream = request.GetRequestStream();
                reqStream.Write(byteArray, 0, byteArray.Length);

                var response = request.GetResponse();

                var respStream = response.GetResponseStream();

                var reader = new StreamReader(respStream);
                string data = reader.ReadToEnd();

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static bool MaxResultSearch(SearchDto searchViewModels)
        {
            ApplicationDbContext appDbContext = new ApplicationDbContext();

            try
            {
                var url = (appDbContext.Settings.First().ApiUrl) + ("api/extract/maxresult");

                var request = WebRequest.Create(url);
                request.Method = "POST";

                var json = JsonConvert.SerializeObject(searchViewModels);
                byte[] byteArray = Encoding.UTF8.GetBytes(json);

                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;

                var reqStream = request.GetRequestStream();
                reqStream.Write(byteArray, 0, byteArray.Length);

                var response = request.GetResponse();

                var respStream = response.GetResponseStream();

                var reader = new StreamReader(respStream);
                string data = reader.ReadToEnd();

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static bool BatchSearch(List<BatchSearchDto> batchSearchViewModels)
        {
            ApplicationDbContext appDbContext = new ApplicationDbContext();

            try
            {
                var url = (appDbContext.Settings.First().ApiUrl) + ("api/extract/batch");

                var request = WebRequest.Create(url);
                request.Method = "POST";

                var json = JsonConvert.SerializeObject(batchSearchViewModels);
                byte[] byteArray = Encoding.UTF8.GetBytes(json);

                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;

                var reqStream = request.GetRequestStream();
                reqStream.Write(byteArray, 0, byteArray.Length);

                var response = request.GetResponse();

                var respStream = response.GetResponseStream();

                var reader = new StreamReader(respStream);
                string data = reader.ReadToEnd();

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
