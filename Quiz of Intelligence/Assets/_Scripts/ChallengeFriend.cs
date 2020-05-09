using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Firebase.Database;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class Invitation
{
    public string sender;
    public string receiver;
}

public class ChallengeFriend : MonoBehaviour
{
    private DatabaseReference _reference;

    public static ChallengeFriend instance = null;


    public static bool isAvailable = true;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        StartCoroutine(CheckForRequest());
        StartCoroutine(WaitFirebaseToLoad());
    }

    IEnumerator WaitFirebaseToLoad()
    {
        yield return new WaitUntil(() => { return FacebookAuthenticator.firebaseInitialized; });
        _reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void InviteFriend(string recID)
    {
        Debug.LogError(recID);
        var inv = new Invitation()
        {
            receiver = recID,
            sender = FacebookAuthenticator.UID
        };
        Debug.LogError(JsonUtility.ToJson(inv));

        _reference.Child("Invitations").Push().SetRawJsonValueAsync(JsonUtility.ToJson(inv));
    }


    IEnumerator CheckForRequest()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            if (isAvailable)
            {
                _reference.Child("Invitations").GetValueAsync().ContinueWith((task) =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Completed");
                        foreach (var dataSnapShot in task.Result.Children.ToList())
                        {
                            var invitation = JsonUtility.FromJson<Invitation>(dataSnapShot.GetRawJsonValue());
                            if (invitation.receiver == FacebookAuthenticator.UID)
                            {
                                Debug.LogError("snap: " + dataSnapShot.GetRawJsonValue());
                                isAvailable = false;
                                // this request is for me
                                Debug.LogError("I was challenged");
                                InvitationReceived(invitation);
                            }
                        }
                    }

                    else
                    {
                        Debug.LogError("Request can not completed");
                    }
                });
            }
        }
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.A))
    //     {
    //         InvitationReceived();
    //     }
    // }

    private void InvitationReceived(Invitation inv)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            var invitationPanel = GameObject.FindGameObjectWithTag("Canvas");
            invitationPanel.transform.GetChild(0).gameObject.SetActive(true);
        });
        // try
        // {
        //     
        // }
        // catch (Exception e)
        // {
        //     Debug.LogError(e);
        //     throw;
        // }
    }

    // private void InvitationReceived()
    // {
    //     var canvas = GameObject.FindGameObjectWithTag("Canvas");
    //     canvas.transform.GetChild(0).gameObject.SetActive(true);
    // }
}