using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
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

    void Start()
    {
        // ---- Configura o slider de volume ----
        float volumeSalvo = PlayerPrefs.GetFloat("volume", 1f);
        sliderVolume.value = volumeSalvo;
        AplicarVolume(volumeSalvo);
        sliderVolume.onValueChanged.AddListener(AplicarVolume);

        // ---- Preenche o dropdown com as resoluções do monitor ----
        resolucoes = Screen.resolutions;
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

    public void AplicarVolume(float valor)
    {
        // converte 0-1 (linear) pra decibéis (logarítmico), que é como o Mixer trabalha
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(valor) * 20);
        PlayerPrefs.SetFloat("volume", valor);
    }

    public void MudarResolucao(int indice)
    {
        Resolution resolucao = resolucoes[indice];
        Screen.SetResolution(resolucao.width, resolucao.height, Screen.fullScreenMode);
    }

    public void AbrirOpcoes()
    {
        painelOpcoes.SetActive(true);
        if (painelMenuPrincipal != null) painelMenuPrincipal.SetActive(false);
    }

    public void VoltarMenu()
    {
        painelOpcoes.SetActive(false);
        if (painelMenuPrincipal != null) painelMenuPrincipal.SetActive(true);
    }
}