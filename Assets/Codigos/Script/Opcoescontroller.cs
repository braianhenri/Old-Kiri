using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public class OpcoesController : MonoBehaviour
{
    [Header("Referências de UI")]
    public GameObject painelOpcoes;
    public GameObject painelMenuPrincipal; // opcional, se quiser esconder o menu atrás

    [Header("Volume")]
    public AudioMixer mainMixer; // arraste o MainMixer aqui
    public Slider sliderVolume;

    [Header("Resolução")]
    public TMP_Dropdown dropdownResolucao;
    private Resolution[] resolucoes;

    [Header("Tela Cheia")]
    public Toggle toggleTelaCheia;

    [Header("Brilho")]
    public Slider sliderBrilho;
    public Volume volumeGlobal; // arraste o Global Volume da cena aqui
    private ColorAdjustments colorAdjustments;

    void Start()
    {
        // ---- Configura o toggle de tela cheia (só roda se estiver conectado) ----
        if (toggleTelaCheia != null)
        {
            bool telaCheiaSalva = PlayerPrefs.GetInt("telaCheia", Screen.fullScreen ? 1 : 0) == 1;
            Screen.fullScreen = telaCheiaSalva;
            toggleTelaCheia.isOn = telaCheiaSalva;
            toggleTelaCheia.onValueChanged.AddListener(MudarTelaCheia);
        }

        // ---- Configura o slider de volume (só roda se estiver conectado) ----
        if (sliderVolume != null)
        {
            float volumeSalvo = PlayerPrefs.GetFloat("volume", 1f);
            sliderVolume.value = volumeSalvo;
            AplicarVolume(volumeSalvo);
            sliderVolume.onValueChanged.AddListener(AplicarVolume);
        }

        // ---- Configura o slider de brilho (só roda se estiver conectado) ----
        if (sliderBrilho != null && volumeGlobal != null &&
            volumeGlobal.profile.TryGet(out colorAdjustments))
        {
            sliderBrilho.minValue = -2f;
            sliderBrilho.maxValue = 2f;

            float brilhoSalvo = PlayerPrefs.GetFloat("brilho", 0f);
            sliderBrilho.value = brilhoSalvo;
            AplicarBrilho(brilhoSalvo);
            sliderBrilho.onValueChanged.AddListener(AplicarBrilho);
        }

        // ---- Preenche o dropdown com as resoluções do monitor, sem duplicatas ----
        if (dropdownResolucao != null)
        {
            Resolution[] todasResolucoes = Screen.resolutions;
            List<Resolution> resolucoesUnicas = new List<Resolution>();
            HashSet<string> vistas = new HashSet<string>();

            foreach (Resolution r in todasResolucoes)
            {
                string chave = r.width + "x" + r.height;
                if (!vistas.Contains(chave))
                {
                    vistas.Add(chave);
                    resolucoesUnicas.Add(r);
                }
            }

            resolucoes = resolucoesUnicas.ToArray();
            dropdownResolucao.ClearOptions();

            List<string> opcoes = new List<string>();
            int indiceAtual = 0;

            for (int i = 0; i < resolucoes.Length; i++)
            {
                string opcao = resolucoes[i].width + " x " + resolucoes[i].height;
                opcoes.Add(opcao);

                if (resolucoes[i].width == Screen.currentResolution.width &&
                    resolucoes[i].height == Screen.currentResolution.height)
                {
                    indiceAtual = i;
                }
            }

            dropdownResolucao.AddOptions(opcoes);
            dropdownResolucao.value = indiceAtual;
            dropdownResolucao.RefreshShownValue();
            dropdownResolucao.onValueChanged.AddListener(MudarResolucao);
        }
    }

    public void AplicarVolume(float valor)
    {
        if (mainMixer != null)
            mainMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(valor, 0.0001f)) * 20);

        PlayerPrefs.SetFloat("volume", valor);
    }

    public void AplicarBrilho(float valor)
    {
        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = valor;

        PlayerPrefs.SetFloat("brilho", valor);
    }

    public void MudarResolucao(int indice)
    {
        if (resolucoes == null || indice < 0 || indice >= resolucoes.Length) return;

        Resolution resolucao = resolucoes[indice];
        Screen.SetResolution(resolucao.width, resolucao.height, Screen.fullScreenMode);
    }

    public void MudarTelaCheia(bool ativo)
    {
        Screen.fullScreen = ativo;
        PlayerPrefs.SetInt("telaCheia", ativo ? 1 : 0);
    }

    public void AbrirOpcoes()
    {
        if (painelOpcoes != null) painelOpcoes.SetActive(true);
        if (painelMenuPrincipal != null) painelMenuPrincipal.SetActive(false);
    }

    public void VoltarMenu()
    {
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
        if (painelMenuPrincipal != null) painelMenuPrincipal.SetActive(true);
    }
}