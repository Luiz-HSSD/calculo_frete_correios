using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using calculo_frete_correios.Droid.br.com.correios.ws;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.IO;
using Plugin.Connectivity;

namespace calculo_frete_correios
{
	public partial class MainPage : ContentPage
	{
        public static string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cep.txt");
        public static string cep_origem = "";
        private static MainPage _pag;

        public static MainPage Pag
        {
            get
            {
                if (_pag == null)
                    _pag = new MainPage();
                return _pag;
            }
            set { _pag = value; }
        }
        private MainPage()
		{
			InitializeComponent();
            if (!File.Exists(fileName))
                File.WriteAllText(fileName, "08561000");
            cep_origem=File.ReadAllText(fileName);
            comp.Items.Add("");
            largura.Items.Add("");
            altura.Items.Add("");
            pesostr.Items.Add("");
            for (int i = 16; i <= 105; i++)
                comp.Items.Add(i.ToString());
            for (int i = 11; i <= 105; i++)
                largura.Items.Add(i.ToString());
            for (int i = 2; i <= 105; i++)
                altura.Items.Add(i.ToString());
            for (int i = 1; i <= 30; i++)
                pesostr.Items.Add(i.ToString());
            cep.Text = "";
            comp.SelectedIndex=0;
            largura.SelectedIndex = 0;
            altura.SelectedIndex = 0;
            pesostr.SelectedIndex = 0;
            
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
            if (!CrossConnectivity.Current.IsConnected)
            {
               await DisplayAlert("alerta", "por favor conecte-se na internet ", "ok");
                return;
            }
            if (cep.Text.Contains("-") && cep.Text.Length < 9)
            {
                await DisplayAlert("alerta", "preencha  corretamente o cep ", "ok");
                return;
            }
            if (!cep.Text.Contains("-") && cep.Text.Length < 8)
            {
                await DisplayAlert("alerta", "preencha corretamente o cep ", "ok");
                return;
            }
            if (comp.SelectedIndex == 0)
            {
                await DisplayAlert("alerta", "preencha o comprimento ", "ok");
                return;
            }
            if (largura.SelectedIndex == 0)
            {
                await DisplayAlert("alerta", "preencha o largura ", "ok");
                return;
            }
            if (altura.SelectedIndex == 0)
            {
                await DisplayAlert("alerta", "preencha o altura ", "ok");
                return;
            }
            if (pesostr.SelectedIndex == 0)
            {
                await DisplayAlert("alerta", "preencha o peso ", "ok");
                return;
            }
            
            string yORn;
            yORn = "N";
            if (avisoRece.IsToggled)
                yORn = "S";
            //string myinput = await InputBox(this.Navigation);
            CalcPrecoPrazoWS ws = new CalcPrecoPrazoWS();
            cResultado c=  ws.CalcPrecoPrazo("", "", "40010 , 41106", cep_origem, cep.Text, pesostr.Items.ElementAt(pesostr.SelectedIndex), 1, int.Parse(comp.Items.ElementAt(comp.SelectedIndex)), int.Parse(altura.Items.ElementAt(altura.SelectedIndex)),int.Parse(largura.Items.ElementAt(largura.SelectedIndex)), 0, "N", 18.5m, yORn);
            if(!string.IsNullOrEmpty(c.Servicos.ElementAt(0).MsgErro))
                await DisplayAlert("Sedex varejo", "Erro: " + c.Servicos.ElementAt(0).MsgErro , "ok");
            else
                await  DisplayAlert("Sedex varejo","preço: "+ c.Servicos.ElementAt(0).Valor+"\nprazo em dias:" + c.Servicos.ElementAt(0).PrazoEntrega, "ok");
            if (!string.IsNullOrEmpty(c.Servicos.ElementAt(1).MsgErro))
                await DisplayAlert("Sedex varejo", "Erro: " + c.Servicos.ElementAt(1).MsgErro, "ok");
            else
                await DisplayAlert("PAC varejo", "preço: " + c.Servicos.ElementAt(1).Valor + "\nprazo em dias:" + c.Servicos.ElementAt(1).PrazoEntrega, "ok");
        }
        public static HttpClient _client = new HttpClient();
        public async void selfDestruct2(object sender, EventArgs args)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("alerta", "por favor conecte-se na internet ", "ok");
                return;
            }

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
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("alerta", "por favor conecte-se na internet ", "ok");
                return;
            }
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
        public async void Bora(object sender, EventArgs args)
        {
            
            Application.Current.MainPage = origem.Pag;

        }
    }
}