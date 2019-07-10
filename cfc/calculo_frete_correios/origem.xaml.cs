using Android;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Connectivity;
using calculo_frete_correios.Droid.CorreiosWS;
using System.Xml;
using System.Xml.Serialization;

namespace calculo_frete_correios
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class origem : ContentPage
    {
        private static origem _pag;

        public static origem Pag
        {
            get
            {
                if (_pag == null)
                    _pag = new origem();
                return _pag;
            }
            set { _pag = value; }
        }

        public IntPtr Handle
        {
            get { return IntPtr.Zero; }
        }

        private origem()
		{
			InitializeComponent();
            if (!Droid.MainActivity.flg_loc_init)
                Droid.MainActivity.loc_init(Droid.MainActivity.main);
            
        }


        public async void Go(object sender, EventArgs args)
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await DisplayAlert("alerta", "por favor conecte-se na internet ", "ok");
                    return;
                }
                if (!string.IsNullOrEmpty(Droid.MainActivity.ss))
                    await DisplayAlert("cordenadas", Droid.MainActivity.ss, "ok");
                else
                {
                    if(Droid.MainActivity.flg_track)
                    {
                        bool cep_p = false;
                        using (var response = await MainPage._client.GetAsync("https://maps.googleapis.com/maps/api/geocode/json?latlng="+ Droid.MainActivity.lat.ToString(CultureInfo.GetCultureInfo("en-US")) + ","+ Droid.MainActivity.lng.ToString(CultureInfo.GetCultureInfo("en-US")) ))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                // Horray it went well!
                                var page = await response.Content.ReadAsStringAsync();
                                JObject jResults = JObject.Parse(page);
                                JArray resultados = (JArray)jResults["results"];
                                
                                foreach (JObject endereco in resultados)
                                {
                                    JArray componetes_endereco =(JArray)endereco["address_components"];
                                    foreach (JObject endpoint in componetes_endereco)
                                    {

                                        JArray types = (JArray)endpoint["types"];
                                        for(int i=0;i<types.Count;i++)//foreach (JObject ty in types)
                                        {
                                            if (types.ElementAt(i).ToString() == "postal_code")
                                            {
                                                cep_p = true;
                                                string s= endpoint["short_name"].ToString();
                                                if (s.Length >= 8)
                                                {
                                                    MainPage.cep_origem = s;
                                                    File.WriteAllText(MainPage.fileName, MainPage.cep_origem);
                                                }
                                                await DisplayAlert("cordenadas", "cep: " + endpoint["short_name"].ToString(), "ok");
                                                break;
                                            }
                                        }
                                    }
                                    break;
                                } 
                            }
                        }
                        if (!cep_p)
                        {
                            string ss= "https://reverse.geocoder.api.here.com/6.2/reversegeocode.json?app_id=IayXi8zl6hLwYJMiPe8I&app_code=X96zYiQMXAHyhhIv34ZuuA&mode=retrieveAddresses&prox=" + Droid.MainActivity.lat.ToString(CultureInfo.GetCultureInfo("en-US")) + "," + Droid.MainActivity.lng.ToString(CultureInfo.GetCultureInfo("en-US")) + ",200";
                            using (var response = await MainPage._client.GetAsync(ss))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    // Horray it went well!
                                    var page = await response.Content.ReadAsStringAsync();
                                    
                                    JObject jResults = JObject.Parse(page);
                                    JArray resultados = (JArray)jResults["Response"]["View"].ElementAt(0)["Result"];

                                    foreach (JObject endereco in resultados)
                                    {
                                        JObject componetes_endereco = (JObject)endereco["Location"]["Address"];


                                        if (componetes_endereco["PostalCode"] != null)
                                        {

                                            string s = componetes_endereco["PostalCode"].ToString();
                                            if (s.Length >= 8)
                                            {
                                                cep_p = true;
                                                MainPage.cep_origem = s; MainPage.cep_origem = s;
                                                File.WriteAllText(MainPage.fileName, MainPage.cep_origem);
                                                
                                            }
                                            await DisplayAlert("cordenadas", "rua: " + componetes_endereco["Street"].ToString() + "\ncidade: " + componetes_endereco["City"].ToString() + "\ncep: " + componetes_endereco["PostalCode"].ToString(), "ok");

                                            break;
                                        }
                                        else
                                            await DisplayAlert("cordenadas", "cep: ", "ok");                                        
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("cordenadas", "problema na requisição", "ok");
                                }
                            }
                            if (!cep_p)
                                await DisplayAlert("cordenadas", "cep indiponível \nlat: " + Droid.MainActivity.lat + "\nlng: " + Droid.MainActivity.lng, "ok");
                        }
                        //await DisplayAlert("cordenadas", "lat: " + Droid.MainActivity.lat + "\nlng: " + Droid.MainActivity.lng, "ok");
                    }
                    else
                        await DisplayAlert("cordenadas", "gps ainda não trackeado", "ok");
                }
                    
            }
            catch(Exception e)
            {
                await DisplayAlert("cordenadas", e.Message.ToString(), "ok");

            }
            //await DisplayAlert("novo endereço escrito", "lat: "+ location.Latitude + "\nEscrito com sucesso", "ok");
        }
        public async void Bora(object sender, EventArgs args)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("alerta", "por favor conecte-se na internet ", "ok");
                return;
            }
            string cepin = await MainPage.InputBox(this.Navigation, "cep", "Digite a cep:");
            using (var response = await MainPage._client.GetAsync("https://viacep.com.br/ws/" + cepin + "/json/"))
            {
                if (response.IsSuccessStatusCode)
                {
                    // Horray it went well!
                    var page = await response.Content.ReadAsStringAsync();
                    MainPage.cep_origem = cepin;
                    File.WriteAllText(MainPage.fileName, MainPage.cep_origem);
                    await DisplayAlert("novo endereço escrito", "cep: " + JObject.Parse(page)["cep"].ToString() + "\nlogradouro: " + JObject.Parse(page)["logradouro"].ToString() + "\nbairro: " + JObject.Parse(page)["bairro"].ToString() + "\ncidade: " + JObject.Parse(page)["localidade"].ToString() + "\nUF: " + JObject.Parse(page)["uf"].ToString() + "\nEscrito com sucesso", "ok");
                }
            }
        }

        XmlSerializer sr = new XmlSerializer(typeof(rastro));
        XmlSerializer srr = new XmlSerializer(typeof(rastroo));

        public async void ras_en(object sender, EventArgs args)
        {
            try 
            { 
                string rasin = await MainPage.InputBox(this.Navigation, "ratreio", "Digite o código de rastreio:");
                if (string.IsNullOrEmpty(rasin))
                {
                    await DisplayAlert("ratreio", "por favor digite o código.","ok");
                    return;
                }
                AtendeClienteService ss = new AtendeClienteService();
                rasin=ss.consultaSRO(new string[] { rasin }, "L", "T", "ECT", "SRO");
                var xr=  XmlReader.Create(new StringReader(rasin));
                rastro ras = (rastro)sr.Deserialize(xr);
            
                xr = XmlReader.Create(new StringReader(rasin));
                rastroo rass = (rastroo)srr.Deserialize(xr);
                ras.objeto.categoria = rass.objeto.categoria;
                ras.objeto.nome = rass.objeto.nome;
                ras.objeto.numero = rass.objeto.numero;
                ras.objeto.sigla = rass.objeto.sigla;
            
                StringBuilder sb = new StringBuilder();
                //sb.Append(ras.objeto.categoria + "      " + rass.objeto.nome + "    " + ras.objeto.numero + "   " + ras.objeto.sigla + "\n\n");
                for (int i=0;i< ras.objeto.Count;i++)
                {
                    sb.Append(ras.objeto.ElementAt(i).local + " " + ras.objeto.ElementAt(i).uf + " "+ ras.objeto.ElementAt(i).cidade + "\t" + ras.objeto.ElementAt(i).codigo+ " " + ras.objeto.ElementAt(i).descricao+"\n");
                    //+ e.data + "   " + e.hora + "   " + e.descricao + "   " + e.status + "\n");
                }
            
                string s = (sb.Length > 0) ? sb.ToString() : "vazio";
                await DisplayAlert(ras.objeto.categoria + "      " + rass.objeto.nome + "    " + ras.objeto.numero, sb.ToString(), "ok");
            }
            catch(Exception e)
            {
                await DisplayAlert("rastreio", e.Message.ToString(), "ok");
            }

        }

#pragma warning disable CS1998 // Este método assíncrono não possui operadores 'await' e será executado de modo síncrono. É recomendável o uso do operador 'await' para aguardar chamadas à API desbloqueadas ou do operador 'await Task.Run(...)' para realizar um trabalho associado à CPU em um thread em segundo plano.
        public async void back(object sender, EventArgs args)
#pragma warning restore CS1998 // Este método assíncrono não possui operadores 'await' e será executado de modo síncrono. É recomendável o uso do operador 'await' para aguardar chamadas à API desbloqueadas ou do operador 'await Task.Run(...)' para realizar um trabalho associado à CPU em um thread em segundo plano.
        {

            Application.Current.MainPage =  MainPage.Pag;

        }


    }
}