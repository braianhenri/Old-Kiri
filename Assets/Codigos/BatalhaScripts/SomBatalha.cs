using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sons da batalha. Cada evento tem um campo de AudioClip: se estiver vazio, toca um som simples
/// gerado por codigo (substituto), e quando voce arrastar um arquivo real ele passa a ser usado.
/// O ControladorCombate cria este componente sozinho; para colocar seus arquivos, adicione
/// "SomBatalha" no mesmo objeto (ControladorCombate) pelo Inspector e arraste os clips.
/// </summary>
public class SomBatalha : MonoBehaviour
{
    public enum Evento
    {
        Selecionar, Confirmar,
        TimingPerfeito, TimingErro,
        Golpe, GolpeCritico, Esquiva, ParteDestruida, Atordoar,
        FacaSaca, Investida, AvisoDesvio, Desvio,
        CorteRecebido, ImpactoRecebido,
        Curar,
        Vitoria, Derrota, Fuga
    }

    [Header("Musica (loop da batalha)")]
    [SerializeField] private AudioClip musicaBatalha;
    [SerializeField, Range(0f, 1f)] private float volumeMusica = 0.5f;

    [Header("Volume geral dos efeitos")]
    [SerializeField, Range(0f, 1f)] private float volumeEfeitos = 0.8f;

    [Header("Menu e escolha de regiao")]
    [SerializeField] private AudioClip selecionar;
    [SerializeField] private AudioClip confirmar;

    [Header("Timing do ataque")]
    [SerializeField] private AudioClip timingPerfeito;
    [SerializeField] private AudioClip timingErro;

    [Header("Ataque do Xomi")]
    [SerializeField] private AudioClip golpe;
    [SerializeField] private AudioClip golpeCritico;
    [SerializeField] private AudioClip esquiva;
    [SerializeField] private AudioClip parteDestruida;
    [SerializeField] private AudioClip atordoar;

    [Header("Ataque do Taison")]
    [SerializeField] private AudioClip facaSaca;
    [SerializeField] private AudioClip investida;
    [SerializeField] private AudioClip avisoDesvio;
    [SerializeField] private AudioClip desvio;
    [SerializeField] private AudioClip corteRecebido;
    [SerializeField] private AudioClip impactoRecebido;

    [Header("Itens e fim da batalha")]
    [SerializeField] private AudioClip curar;
    [SerializeField] private AudioClip vitoria;
    [SerializeField] private AudioClip derrota;
    [SerializeField] private AudioClip fuga;

    private const int Taxa = 44100;

    private AudioSource fonteEfeitos;
    private AudioSource fonteMusica;
    private readonly Dictionary<Evento, AudioClip> gerados = new Dictionary<Evento, AudioClip>();

    private void Awake()
    {
        fonteEfeitos = gameObject.AddComponent<AudioSource>();
        fonteEfeitos.playOnAwake = false;
        fonteEfeitos.spatialBlend = 0f;

        fonteMusica = gameObject.AddComponent<AudioSource>();
        fonteMusica.playOnAwake = false;
        fonteMusica.spatialBlend = 0f;
        fonteMusica.loop = true;
    }

    private void Start()
    {
        RecomecarMusica();
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<Evento, AudioClip> par in gerados)
        {
            if (par.Value != null) Destroy(par.Value);
        }
        gerados.Clear();
    }

    public void RecomecarMusica()
    {
        if (fonteMusica == null || musicaBatalha == null) return;
        fonteMusica.clip = musicaBatalha;
        fonteMusica.volume = volumeMusica;
        fonteMusica.Stop();
        fonteMusica.Play();
    }

    public void Tocar(Evento evento)
    {
        if (fonteEfeitos == null) return;

        AudioClip clip = Obter(evento);
        if (clip != null) fonteEfeitos.PlayOneShot(clip, volumeEfeitos);

        if ((evento == Evento.Vitoria || evento == Evento.Derrota || evento == Evento.Fuga) && fonteMusica != null)
            fonteMusica.Stop();
    }

    private AudioClip Obter(Evento evento)
    {
        AudioClip real = null;
        switch (evento)
        {
            case Evento.Selecionar: real = selecionar; break;
            case Evento.Confirmar: real = confirmar; break;
            case Evento.TimingPerfeito: real = timingPerfeito; break;
            case Evento.TimingErro: real = timingErro; break;
            case Evento.Golpe: real = golpe; break;
            case Evento.GolpeCritico: real = golpeCritico; break;
            case Evento.Esquiva: real = esquiva; break;
            case Evento.ParteDestruida: real = parteDestruida; break;
            case Evento.Atordoar: real = atordoar; break;
            case Evento.FacaSaca: real = facaSaca; break;
            case Evento.Investida: real = investida; break;
            case Evento.AvisoDesvio: real = avisoDesvio; break;
            case Evento.Desvio: real = desvio; break;
            case Evento.CorteRecebido: real = corteRecebido; break;
            case Evento.ImpactoRecebido: real = impactoRecebido; break;
            case Evento.Curar: real = curar; break;
            case Evento.Vitoria: real = vitoria; break;
            case Evento.Derrota: real = derrota; break;
            case Evento.Fuga: real = fuga; break;
        }
        if (real != null) return real;

        AudioClip gerado;
        if (!gerados.TryGetValue(evento, out gerado))
        {
            gerado = Criar(evento);
            gerados[evento] = gerado;
        }
        return gerado;
    }

    // ================= sons gerados por codigo (substitutos) =================

    private static AudioClip Criar(Evento evento)
    {
        float[] d;
        switch (evento)
        {
            case Evento.Selecionar:
                d = Tom(880f, 880f, 0.04f, 0f, 6f);
                break;
            case Evento.Confirmar:
                d = Tom(520f, 880f, 0.1f, 0.3f, 5f);
                break;
            case Evento.TimingPerfeito:
                d = Seq(Tom(988f, 988f, 0.09f, 0f, 4f), Tom(1319f, 1319f, 0.3f, 0f, 5f));
                break;
            case Evento.TimingErro:
                d = Tom(200f, 80f, 0.28f, 0.7f, 4f);
                break;
            case Evento.Golpe:
                d = Mix(Tom(160f, 45f, 0.2f, 0f, 6f), Esc(Ruido(0.08f, 0.5f, 0.02f, 6f), 0.6f));
                break;
            case Evento.GolpeCritico:
                d = Mix(Tom(200f, 40f, 0.3f, 0.2f, 5f), Esc(Ruido(0.15f, 0.7f, 0.02f, 5f), 0.8f),
                        Esc(Atraso(Tom(1319f, 1760f, 0.25f, 0f, 5f), 0.05f), 0.5f));
                break;
            case Evento.Esquiva:
                d = Esc(Ruido(0.28f, 0.25f, 0.5f, 4f), 0.6f);
                break;
            case Evento.ParteDestruida:
                d = Mix(Ruido(0.4f, 0.35f, 0.02f, 4f), Tom(110f, 40f, 0.4f, 0.4f, 4f));
                break;
            case Evento.Atordoar:
                d = Tom(520f, 260f, 0.5f, 0.1f, 2.5f, 0.08f);
                break;
            case Evento.FacaSaca:
                d = Mix(Esc(Ruido(0.18f, 0.9f, 0.6f, 6f), 0.5f), Esc(Tom(2200f, 3200f, 0.18f, 0f, 6f), 0.4f));
                break;
            case Evento.Investida:
                d = Mix(Esc(Ruido(0.2f, 0.2f, 0.6f, 4f), 0.7f), Atraso(Tom(140f, 50f, 0.15f, 0f, 6f), 0.12f));
                break;
            case Evento.AvisoDesvio:
                d = Seq(Tom(1000f, 1000f, 0.07f, 0.5f, 3f), Silencio(0.05f), Tom(1000f, 1000f, 0.07f, 0.5f, 3f));
                break;
            case Evento.Desvio:
                d = Esc(Ruido(0.16f, 0.3f, 0.4f, 5f), 0.5f);
                break;
            case Evento.CorteRecebido:
                d = Mix(Esc(Ruido(0.14f, 0.9f, 0.05f, 6f), 0.7f), Tom(320f, 90f, 0.18f, 0.3f, 6f));
                break;
            case Evento.ImpactoRecebido:
                d = Mix(Tom(130f, 40f, 0.22f, 0f, 5f), Esc(Ruido(0.1f, 0.4f, 0.02f, 6f), 0.6f));
                break;
            case Evento.Curar:
                d = Seq(Tom(523f, 523f, 0.08f, 0f, 4f), Tom(659f, 659f, 0.08f, 0f, 4f), Tom(784f, 784f, 0.2f, 0f, 4f));
                break;
            case Evento.Vitoria:
                d = Seq(Tom(523f, 523f, 0.14f, 0f, 3f), Tom(659f, 659f, 0.14f, 0f, 3f),
                        Tom(784f, 784f, 0.14f, 0f, 3f), Tom(1047f, 1047f, 0.45f, 0f, 3f));
                break;
            case Evento.Derrota:
                d = Seq(Tom(392f, 392f, 0.25f, 0.2f, 3f), Tom(330f, 330f, 0.25f, 0.2f, 3f),
                        Tom(262f, 262f, 0.25f, 0.2f, 3f), Tom(196f, 196f, 0.6f, 0.3f, 2.5f));
                break;
            default: // Fuga
                d = Mix(Esc(Ruido(0.45f, 0.2f, 0.3f, 3f), 0.5f), Esc(Tom(500f, 200f, 0.4f, 0f, 3f), 0.3f));
                break;
        }

        Normalizar(d, 0.9f);
        AudioClip clip = AudioClip.Create("Gerado_" + evento, d.Length, 1, Taxa, false);
        clip.SetData(d, 0);
        return clip;
    }

    // tom com varredura de frequencia (f0 -> f1), "quadrada" mistura onda quadrada, "decaimento" apaga o som
    private static float[] Tom(float f0, float f1, float dur, float quadrada, float decaimento, float vibrato = 0f)
    {
        int n = Mathf.Max(1, (int)(dur * Taxa));
        float[] d = new float[n];
        float fase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float f = Mathf.Lerp(f0, f1, t) * (1f + vibrato * Mathf.Sin(t * dur * 40f));
            fase += 2f * Mathf.PI * f / Taxa;
            float s = Mathf.Sin(fase);
            if (quadrada > 0f) s = Mathf.Lerp(s, Mathf.Sign(s) * 0.6f, quadrada);
            float env = Mathf.Exp(-decaimento * t) * Mathf.Min(1f, i / 80f);
            d[i] = s * env;
        }
        return d;
    }

    // ruido: "suavizar" 1 = chiado agudo, perto de 0 = grave; "subida" = fracao do som ate o pico
    private static float[] Ruido(float dur, float suavizar, float subida, float decaimento)
    {
        int n = Mathf.Max(1, (int)(dur * Taxa));
        float[] d = new float[n];
        System.Random rng = new System.Random(1234 + n);
        float anterior = 0f;
        for (int i = 0; i < n; i++)
        {
            float r = (float)(rng.NextDouble() * 2.0 - 1.0);
            anterior += (r - anterior) * suavizar;
            float t = (float)i / n;
            float env = t < subida ? t / Mathf.Max(0.0001f, subida) : Mathf.Exp(-decaimento * (t - subida) / Mathf.Max(0.0001f, 1f - subida));
            d[i] = anterior * env;
        }
        return d;
    }

    private static float[] Silencio(float dur)
    {
        return new float[Mathf.Max(1, (int)(dur * Taxa))];
    }

    private static float[] Atraso(float[] a, float seg)
    {
        return Seq(Silencio(seg), a);
    }

    private static float[] Esc(float[] a, float ganho)
    {
        float[] r = new float[a.Length];
        for (int i = 0; i < a.Length; i++) r[i] = a[i] * ganho;
        return r;
    }

    private static float[] Mix(params float[][] partes)
    {
        int n = 0;
        for (int p = 0; p < partes.Length; p++) n = Mathf.Max(n, partes[p].Length);
        float[] r = new float[n];
        for (int p = 0; p < partes.Length; p++)
            for (int i = 0; i < partes[p].Length; i++) r[i] += partes[p][i];
        return r;
    }

    private static float[] Seq(params float[][] partes)
    {
        int n = 0;
        for (int p = 0; p < partes.Length; p++) n += partes[p].Length;
        float[] r = new float[n];
        int pos = 0;
        for (int p = 0; p < partes.Length; p++)
        {
            System.Array.Copy(partes[p], 0, r, pos, partes[p].Length);
            pos += partes[p].Length;
        }
        return r;
    }

    private static void Normalizar(float[] d, float pico)
    {
        float max = 0f;
        for (int i = 0; i < d.Length; i++) max = Mathf.Max(max, Mathf.Abs(d[i]));
        if (max < 0.0001f) return;
        float g = pico / max;
        for (int i = 0; i < d.Length; i++) d[i] *= g;
    }
}
