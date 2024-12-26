using Ferramentas.Desktop.Models.Enumerados;
using SukiUI.Helpers;

namespace Ferramentas.Desktop.Models;

public class QuestãoDoPullRequest : SukiObservableObject
{
    private string descrição;
    private int ordem;
    private string pergunta;
    private string resposta;
    private TipoDaQuestão tipoDaQuestão;

    public string Pergunta
    {
        get => pergunta;
        set => SetAndRaise(ref pergunta, value);
    }

    public string Resposta
    {
        get => resposta;
        set => SetAndRaise(ref resposta, value);
    }

    public int Ordem
    {
        get => ordem;
        set => SetAndRaise(ref ordem, value);
    }

    public TipoDaQuestão TipoDaQuestão
    {
        get => tipoDaQuestão;
        set => SetAndRaise(ref tipoDaQuestão, value);
    }

    public string Descrição
    {
        get => descrição;
        set => SetAndRaise(ref descrição, value);
    }
}