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
            
            
        }


        public async void Go(object sender, EventArgs args)
        {
            try
            {
                if (!string.IsNullOrEmpty(Droid.MainActivity.ss))
                    await DisplayAlert("cordenadas", Droid.MainActivity.ss, "ok");
                else
                {
                    if(calculo_frete_correios.Droid.MainActivity.flg_track)
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
                                
                                foreach (JObject bank in resultados)
                                {
                                    JArray jResults_bank_endpoint =(JArray)bank["address_components"];
                                    foreach (JObject endpoint in jResults_bank_endpoint)
                                    {

                                        JArray types = (JArray)endpoint["types"];
                                        for(int i=0;i<types.Count;i++)//foreach (JObject ty in types)
                                        {
                                            if (types.ElementAt(i).ToString() == "postal_code")
                                            {
                                                cep_p = true;
                                                await DisplayAlert("cordenadas", "cep: " + endpoint["short_name"].ToString(), "ok");
                                            }
                                        }
                                    }
                                    break;
                                } 
                            }
                        }
                        if(!cep_p)
                            await DisplayAlert("cordenadas", "cep indiponível \nlat: " + Droid.MainActivity.lat + "\nlng: " + Droid.MainActivity.lng, "ok");
                        //await DisplayAlert("cordenadas", "lat: " + Droid.MainActivity.lat + "\nlng: " + Droid.MainActivity.lng, "ok");
                    }
                    else
                        await DisplayAlert("cordenadas", "gps ainda não trackeado", "ok");
                }
                    
            }
            catch(Exception e)
            {
                await DisplayAlert("cordenadas", e.ToString(), "ok");
            }
            //await DisplayAlert("novo endereço escrito", "lat: "+ location.Latitude + "\nEscrito com sucesso", "ok");
        }
        public async void Bora(object sender, EventArgs args)
        {

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
        public async void back(object sender, EventArgs args)
        {

            Application.Current.MainPage = MainPage.Pag;

        }


    }
}