using UnityEngine;
using UnityEngine.UI;

// 実装意図: Tikuwa の HP 変更イベントを受けて、UI Image の fillAmount だけを更新する表示専用 component。
public class PlayerHPUI : MonoBehaviour
{
    public Tikuwa player;
    public Image hpBarImage;
    public float emptyFillAmount = 0f;
    public float fullFillAmount = 1f;

    void OnEnable()
    {
        // 実装意図: Inspector 参照が未設定でも Player タグから最低限動くように自動解決する。
        if(player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if(playerObject != null) player = playerObject.GetComponent<Tikuwa>();
        }

        if(hpBarImage == null) hpBarImage = GetComponent<Image>();

        if(hpBarImage != null)
        {
            hpBarImage.type = Image.Type.Filled;
            hpBarImage.fillMethod = Image.FillMethod.Horizontal;
            hpBarImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        if(player != null)
        {
            player.OnHpChanged += UpdateHpBar;
            UpdateHpBar(player.CurrentHP, player.MaxHP);
        }
    }

    void OnDisable()
    {
        if(player != null) player.OnHpChanged -= UpdateHpBar;
    }

    void UpdateHpBar(int currentHP, int maxHP)
    {
        if(hpBarImage == null) return;

        float rate = maxHP <= 0 ? 0f : Mathf.Clamp01((float)currentHP / maxHP);
        hpBarImage.fillAmount = Mathf.Lerp(emptyFillAmount, fullFillAmount, rate);
    }
}
