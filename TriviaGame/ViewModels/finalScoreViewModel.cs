using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TriviaGame.Models;
using TriviaGame.Services;

namespace TriviaGame.ViewModels
{
    public class finalScoreViewModel : propertiesChangesViewModel
    {
        private queryService queryService;

        public RelayCommand menuPrinCommand { get; }
        public ObservableCollection<scoreModel> Puntuaciones { get; } = new ObservableCollection<scoreModel>();

        public finalScoreViewModel(Action volverAMenu)
        {
            menuPrinCommand = new RelayCommand(volverAMenu);
            queryService = new queryService();
            CargarPuntuaciones();
        }

        private async void CargarPuntuaciones()
        {
            var datos = await queryService.GetPuntuaciones();
            Puntuaciones.Clear();
            foreach (var d in datos)
            {
                Puntuaciones.Add(new scoreModel
                {
                    Jugador = d["nombreJugador"].ToString(),
                    Categoria = d["NombreCategoria"].ToString(),
                    Puntuacion = Convert.ToInt32(d["puntuacionTotal"]),
                    Aciertos = Convert.ToInt32(d["puntuacionTotal"]) / 10
                });
            }
        }
    }
}
