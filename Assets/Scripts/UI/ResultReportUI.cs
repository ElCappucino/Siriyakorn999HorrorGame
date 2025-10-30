using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultReportUI : MonoBehaviour
{
    [SerializeField] private TMP_Text HPBonusNumText;
    [SerializeField] private TMP_Text TalismanWrittenNumText;
    [SerializeField] private TMP_Text GhostExorcistedNumText;
    [SerializeField] private TMP_Text FinalScoreText;
    [SerializeField] private int hpBonusMultiplier;

    public void UpdateText(int score, int hpleft, int talismanWritten, int ghostExorcisted)
    {
        int hpBonus = hpleft * hpBonusMultiplier;
        HPBonusNumText.text = hpleft.ToString() + " x " + hpBonusMultiplier.ToString() + " = " + hpBonus.ToString();

        TalismanWrittenNumText.text = talismanWritten.ToString();
        GhostExorcistedNumText.text = ghostExorcisted.ToString();
        FinalScoreText.text = score.ToString();
    }
}
