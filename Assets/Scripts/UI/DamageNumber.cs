using TMPro;
using UnityEngine;



public class DamageNumber : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Gradient colorByDamage;
    [SerializeField] float lifetime = 2f, fadeFromTime = 1.5f;
    float time = 0f;
    float fadeTotalTime;



    void Start()
    {
        fadeTotalTime = lifetime - fadeFromTime;
    }


    //to add:
    //if any effects, generate gradient with equally-spaced keys (1 key for each effect's color) and use it as an outline/backdrop
    public void SetData(float dmg)
    {
        SetData(dmg, .5f);
    }
    public void SetData(float dmg, float dmgEfficacy)
    {
        //float dmgAlpha = dmg / 100f;
        text.text = dmg.ToString();

        text.color = colorByDamage.Evaluate(dmgEfficacy);

        float dmgScale = 0.5f + dmgEfficacy;
        text.transform.localScale = new Vector3(dmgScale, dmgScale, dmgScale);
    }



    void Update()
    {
        transform.LookAt(Player.instance.cam.transform);

        time += Time.deltaTime;

        if (time > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (time > fadeFromTime)
        {
            text.alpha = Mathf.Lerp(1f, 0f, (time - fadeFromTime) / fadeTotalTime);
        }
    }
}
