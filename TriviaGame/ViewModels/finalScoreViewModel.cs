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

public finalScoreViewModel(Action volverAMenu, int idCategoria)
    {
        menuPrinCommand = new RelayCommand(volverAMenu);
        queryService = new queryService();
        CargarPuntuaciones(idCategoria);
        }

        private async void CargarPuntuaciones(int idCategoria)
        {
            var datos = await queryService.GetPuntuacionesPorCategoria(idCategoria);
            Puntuaciones.Clear();
            if (datos == null) return;
            foreach (var d in datos)
            {
                Puntuaciones.Add(new scoreModel
                {
                    Jugador = d.ContainsKey("nombreJugador") ? d["nombreJugador"]?.ToString() ?? "Desconocido" : "Desconocido",
                    Categoria = d.ContainsKey("NombreCategoria") ? d["NombreCategoria"]?.ToString() ?? "General" : "General",
                    Puntuacion = d.ContainsKey("puntuacionTotal") ? Convert.ToInt32(d["puntuacionTotal"]) : 0,
                    Aciertos = d.ContainsKey("puntuacionTotal") ? Convert.ToInt32(d["puntuacionTotal"]) / 10 : 0
                });
            }
        }
    }
}
