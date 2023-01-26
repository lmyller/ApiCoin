using ApiCoin.Model;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace ApiCoin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinController : ControllerBase
    {
        public const string DEFAULT_URL = "https://fxtop.com/pt/historico-das-taxas-de-cambio.php?YA=1&C1=USD&C2={0}L&A=1&YYYY1={1}&MM1=01&DD1=01&YYYY2={2}&MM2=12&DD2=30&LANG=en";

        [HttpGet("/v1/coin/{coin}/start_year{startYear:int}-end_year{endYear:int}")]
        public ActionResult<string> Get(string coin, int startYear, int endYear)
        {
            string json;
            
            if ((json = GetHistoryCoin(coin, startYear, endYear)) != null)
                return Ok(json);

            return BadRequest();
        }

        private string GetHistoryCoin(string coin, int startYear, int endYear)
        {
            List<Coin> _coin;
            HtmlWeb web = CreateHtmlWeb();

            coin = coin.ToUpper();

            var htmlDoc = web.Load(String.Format(DEFAULT_URL, coin, startYear, endYear));

            var doc = htmlDoc.DocumentNode.SelectSingleNode("//table[@border=1]").Descendants("td");

            _coin = GetListCoin(doc);

            return JsonSerializer.Serialize(_coin);
        }

        private List<Coin> GetListCoin(IEnumerable<HtmlNode> doc)
        {
            List<int> listYear = new List<int>();
            List<double> listValue = new List<double>();
            int columnTable = 0;

            foreach (var node in doc)
            {
                string value = node.OuterHtml;
                value = value.Replace("<td>", "").Replace("</td>", "");
         
                if (value[0] is not '<')
                {
                    columnTable++;

                    if (columnTable == 1)
                        listYear.Add(int.Parse(value));

                    if (columnTable == 2)
                        listValue.Add(Double.Parse(value, CultureInfo.InvariantCulture));

                    if (columnTable == 5)
                        columnTable = 0;
                }
            }

            return CreateListCoin(listYear, listValue);
        }

        private List<Coin> CreateListCoin(List<int> listYear, List<double> listValue)
        {
            List<Coin> _coin = new List<Coin>();

            for (int i = 0; i < listYear.Count; i++)
            {
                _coin.Add(new Coin()
                {
                    Year = listYear[i],
                    Value = listValue[i]
                });
            }

            return _coin;
        }

        private HtmlWeb CreateHtmlWeb()
        {
            return new HtmlWeb();
        }
    }
}
