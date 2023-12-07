using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Erinn
{
    public sealed class MainEntry : MonoBehaviour
    {
        public string Address;
        public uint Port;
        public Image Background;
        public GameObject LoginUI;
        private MessageManager _messageManager;
        private ITimerHandle _reconnectTimer;

        private void Awake()
        {
            Screen.sleepTimeout = 30;
            Application.targetFrameRate = 30;
            DontDestroyOnLoad(gameObject);
            _reconnectTimer = Entry.Timer.GetHandler();
        }

        private void Start()
        {
            Screen.SetResolution(400, 880, FullScreenMode.Windowed);
            var client = NetworkTransport.Singleton;
            client.ChangeLog(false);
            _messageManager = new MessageManager(client.Master);
            Entry.Timer.Create(2f, () =>
            {
                LoginUI.SetActive(true);
                Background.DOFade(0f, 2f);
                Entry.Timer.Create(2f, ConnectToMaster);
            });
        }

        private void ConnectToMaster()
        {
            Background.gameObject.SetActive(false);
            var client = NetworkTransport.Singleton;
            client.OnConnected += OnConnected;
            client.OnDisconnected += OnDisconnected;
            Entry.Timer.Create(2f, () => client.Connect(Address, Port));
        }

        private void OnConnected()
        {
            _reconnectTimer.Stop();
        }

        private void OnDisconnected()
        {
            if (!Application.isPlaying)
                return;
            if (!Entry.IsRuntime)
                return;
            _reconnectTimer.Create(5f, () =>
            {
                if (Application.isPlaying)
                    NetworkTransport.Singleton.Connect("127.0.0.1", 7777);
            });
        }
    }
}