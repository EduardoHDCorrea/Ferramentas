using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Ferramentas.Desktop.Models.Enumerados;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SukiUI.Helpers;

namespace Ferramentas.Desktop.Models;

public class ModeloDoMarkdown : SukiObservableObject
{
    public ModeloDoMarkdown(string arquivoJson)
    {
        Questões = [];
        var conteúdoJson = File.ReadAllText(arquivoJson);
        var seções = JsonConvert.DeserializeObject<List<SeçãoDaQuestãoJson>>(conteúdoJson);
        if (seções is null)
            return;

        seções.ForEach(
            x => Questões.Add(
                new QuestãoDoPullRequest
                {
                    Pergunta = x.Pergunta,
                    Resposta = x.Resposta ?? "",
                    Descrição = x.Descrição,
                    Ordem = x.Ordem,
                    TipoDaQuestão = x.Tipo
                }
            )
        );
    }

    public ObservableCollection<QuestãoDoPullRequest> Questões { get; set; }

    public class SeçãoDaQuestãoJson
    {
        public string Pergunta { get; set; }
        public string? Resposta { get; set; }
        public string Descrição { get; set; }
        public int Ordem { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TipoDaQuestão Tipo { get; set; }
    }
}