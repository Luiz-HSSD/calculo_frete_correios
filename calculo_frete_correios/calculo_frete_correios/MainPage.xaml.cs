using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using calculo_frete_correios.Droid.br.com.correios.ws;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace calculo_frete_correios
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}
        public static Task<string> InputBox(INavigation navigation,string title,string message)
        {
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<string>();

            var lblTitle = new Label { Text = title, HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = message };
            var txtInput = new Entry { Text = "" };

            var btnOk = new Button
            {
                Text = "Ok",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                // close page
                var result = txtInput.Text;
                await navigation.PopModalAsync();
                // pass result
                tcs.SetResult(result);
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                WidthRequest = 100,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                // close page
                await navigation.PopModalAsync();
                // pass empty result
                tcs.SetResult(null);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, txtInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            txtInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }
        public async void selfDestruct(object sender, EventArgs args)
        {
            
            //string myinput = await InputBox(this.Navigation);
            CalcPrecoPrazoWS ws = new CalcPrecoPrazoWS();
            cResultado c=  ws.CalcPrecoPrazo("", "", "40010 , 40045 , 40215 , 40290 , 41106", "08563010", cep.Text, pesostr.Text, 1, int.Parse(comp.Text), int.Parse(altura.Text),int.Parse(largura.Text), 0, "N", 18.5m, "S");
            await  DisplayAlert("Sedex varejo","preço: "+ c.Servicos.ElementAt(0).Valor+"\nprazo em dias:" + c.Servicos.ElementAt(0).PrazoEntrega, "ok"); //"Would you like to play a game", "Yes", "No");
        }
        static HttpClient _client = new HttpClient();
        public async void selfDestruct2(object sender, EventArgs args)
        {

            string uf = await InputBox(this.Navigation,"uf","Digite a uf:");
            string cidade = await InputBox(this.Navigation,"cidade","Digite a cidade:");
            string rua = await InputBox(this.Navigation,"logradouro", "Digite o logradouro(rua, avenida,estrada,etc...):");
            using (var response = await _client.GetAsync("https://viacep.com.br/ws/"+uf+"/"+cidade+"/"+rua+"/json/"))
            {
                if (response.IsSuccessStatusCode)
                {
                    // Horray it went well!
                    var page = await response.Content.ReadAsStringAsync();
                    cep.Text= JArray.Parse(page)[0]["cep"].ToString();
                    await DisplayAlert("endereco ","cep: "+JArray.Parse(page)[0]["cep"].ToString(), "ok"); 
                }
            }
            
        }
        public async void selfDestruct3(object sender, EventArgs args)
        {

            string cepin = await InputBox(this.Navigation, "cep", "Digite a cep:");
            using (var response = await _client.GetAsync("https://viacep.com.br/ws/" + cepin + "/json/"))
            {
                if (response.IsSuccessStatusCode)
                {
                    // Horray it went well!
                    var page = await response.Content.ReadAsStringAsync();
                    cep.Text = JObject.Parse(page)["cep"].ToString();
                    await DisplayAlert("endereco ", "cep: " + JObject.Parse(page)["cep"].ToString()+ "\nlogradouro: "+ JObject.Parse(page)["logradouro"].ToString() + "\nbairro: " + JObject.Parse(page)["bairro"].ToString() + "\ncidade: " + JObject.Parse(page)["localidade"].ToString() + "\nUF: " + JObject.Parse(page)["uf"].ToString(), "ok");
                }
            }

        }
    }
}
