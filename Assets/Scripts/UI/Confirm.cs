using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Confirm : MonoBehaviour
{
    [SerializeField]
    private Button ConfirmButton;
    [SerializeField]
    private Button CancelButton;

    [SerializeField]
    private UnityEvent OnActivated;
    [SerializeField]
    private UnityEvent OnConfirmed;
    [SerializeField]
    private UnityEvent OnCanceled;

    void Start()
    {
        ConfirmButton.onClick.AddListener(this.DoConfirm);
        CancelButton.onClick.AddListener(this.DoCancel);
    }

    public void StartConfirm()
    {
        this.gameObject.SetActive(true);
        OnActivated?.Invoke();
    }

    public void DoConfirm()
    {
        OnConfirmed?.Invoke();
        this.gameObject.SetActive(false);
    }

    public void DoCancel()
    {
        OnCanceled?.Invoke();
        this.gameObject.SetActive(false);
    }
}
