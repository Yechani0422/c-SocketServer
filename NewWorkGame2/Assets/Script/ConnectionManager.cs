using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Data;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;



public class ConnectionManager : MonoBehaviour
{
    #region
    private Socket mSocket;
    private byte[] mReadBuffer = new byte[CVSP.SpecificationCVSP.CVSP_BUFFERSIZE];
    private BinaryFormatter mBinaryFormat;
    private bool mNewUser;
    private ArrayList mTransBuffer;
    public bool CONNECT { get; private set; }
    #endregion

    #region
    public InputField playerInput;
    bool isJoinRequested = false;
    bool isGameStart = false;
    bool isPrevGameStart = false;
    string playerName = "";
    int tokenID;
    

    public string chatMessege;
    Text chatText;
    ScrollRect scroll_rect = null;
    Queue<string> queueChat = new Queue<string>();

    public ArrayList playerLists=new ArrayList();
    string enemyName = "";

    Queue<CVSP.PlayerInfo> queueEnemies = new Queue<CVSP.PlayerInfo>();
    Queue<CVSP.AttackInfo> queueAttacks = new Queue<CVSP.AttackInfo>();
    Queue<CVSP.HpInfo> queueHp = new Queue<CVSP.HpInfo>();
    Queue<CVSP.ScoreInfo> queueScore = new Queue<CVSP.ScoreInfo>();

    [SerializeField]
    GameObject scroeBoard;

    public GameObject player;

    public void SetPlayerName()
    {
        Debug.Log(playerInput.text + "를 입력 하셨습니다!");

        if (isGameStart == false&&isJoinRequested==false)
        {
            playerName = playerInput.text;
            playerInput.text = string.Empty;
            GameObject.Find("Placeholder").GetComponent<Text>().text = "채팅입력!";

            if(!ConnectionServer("127.0.0.1"))
            {
                Debug.Log("서버와 연결이 실패 했습니다.");
                return;
            }
            //isGameStart = true;

            SendWithPayLoad(CVSP.SpecificationCVSP.CVSP_JOINREQ, CVSP.SpecificationCVSP.CVSP_SUCCESS, playerName);

            isJoinRequested = true;
        }
        else if(isGameStart==false&&isJoinRequested==true)
        {
            Debug.Log("서버에 로그인 중입니다 응답을 기다려 주세요!");
            return;
        }
        else if( isGameStart&&CONNECT&&isJoinRequested)
        {
            chatMessege = playerInput.text;
            playerInput.text = string.Empty;

            SendWithPayLoad(CVSP.SpecificationCVSP.CVSP_CHATTINGREQ, CVSP.SpecificationCVSP.CVSP_SUCCESS, chatMessege);
        }
    }
    #endregion

    public void showChat(string chat)
    {
        chatText.text += chat + "\n";
        scroll_rect.verticalNormalizedPosition = 0.0f;
    }

    void Awake()
    {
        chatText = GameObject.Find("ChatText").GetComponent<Text>();
        scroll_rect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();
    }

    private void Update()
    {
        if(queueChat.Count>0)
        {
            showChat(queueChat.Dequeue());
        }

        if (enemyName != "")
        {
            StartCoroutine(CreateEnemy(enemyName));
            enemyName = "";
        }

        if(queueEnemies.Count>0)
        {
            CVSP.PlayerInfo info = queueEnemies.Dequeue();

            foreach(GameObject enemy in playerLists)
            {
                Debug.Log("enemy=" + enemy.GetComponent<PlayerCtrl>().GetPlayerName());
                Debug.Log("info.id=" + info.id);

                if(enemy.GetComponent<PlayerCtrl>().GetPlayerName().Contains(info.id))
                {
                    Debug.Log("여기서 업데이트 수행");
                    Vector3 pos = new Vector3(info.posX, info.posY, info.posZ);
                    Quaternion rotation = new Quaternion(info.quatX, info.quatY, info.quatZ, info.quatW);

                    //enemy.transform.position = pos;
                    //enemy.transform.rotation = rotation;
                    if (enemy.transform.position != pos)
                    {
                        enemy.GetComponent< PlayerCtrl>().animator.SetFloat("Speed", 1.0f);
                    }
                    else
                    {
                        enemy.GetComponent<PlayerCtrl>().animator.SetFloat("Speed", 0.0f);
                    }
                    enemy.transform.position = Vector3.Lerp(enemy.transform.position, pos, Time.deltaTime * 10.0f);
                    enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation, rotation, 0.01f * 10.0f);
                }
            }
        }

        if(isGameStart&&CONNECT&&isJoinRequested&&isPrevGameStart==false)
        {
            StartCoroutine(CreatePlayer(playerName));
        }

        if(queueAttacks.Count>0)
        {
            CVSP.AttackInfo info = queueAttacks.Dequeue();

            foreach(GameObject enemy in playerLists)
            {
                if(enemy.GetComponent<PlayerCtrl>().GetPlayerName().Contains(info.id))
                {
                    Vector3 pos = new Vector3(info.posX, info.posY, info.posZ);
                    Quaternion rotation = new Quaternion(info.quatX, info.quatY, info.quatZ, info.quatW);

                    enemy.GetComponent<PlayerCtrl>().firePos.transform.position = pos;
                    enemy.GetComponent<PlayerCtrl>().firePos.transform.rotation = rotation;
                    enemy.GetComponent<PlayerCtrl>().FireRPC();
                    break;
                }
            }
        }

        if(queueHp.Count>0)
        {
            CVSP.HpInfo info = queueHp.Dequeue();

            foreach(GameObject enemy in playerLists)
            {
                if(enemy.GetComponent<PlayerCtrl>().GetPlayerName().Contains(info.id))
                {
                    int hp = info.hp;
                    string bulletOwner = info.bulletOwner;

                    if(enemy.GetComponent<PlayerCtrl>().hp>hp)
                    {
                        enemy.GetComponent<PlayerCtrl>().hp = hp;
                        enemy.GetComponent<PlayerCtrl>().animator.SetTrigger("Hit");
                    }
                    
                    
                    
                    if(enemy.GetComponent<PlayerCtrl>().hp<=0)
                    {
                        enemy.GetComponent<PlayerCtrl>().animator.SetTrigger("Die");
                        enemy.GetComponent<PlayerCtrl>().RespawnRPC();
                        string killInfo= bulletOwner + " 님이" + info.id + "님을 죽였습니다!";
                        PlayerCtrl[] owner =FindObjectsOfType<PlayerCtrl>();
                        foreach(PlayerCtrl player in owner)
                        {
                            if(player.playerName.text==bulletOwner)
                            {
                                player.Kills += 1;
                                SendMessage("Score", player);
                            }
                        }
                        SendWithPayLoad(CVSP.SpecificationCVSP.CVSP_CHATTINGREQ, CVSP.SpecificationCVSP.CVSP_SUCCESS, killInfo);
                    }
                    
                    break;
                }
            }
        }

        if (queueScore.Count > 0)
        {
            CVSP.ScoreInfo info = queueScore.Dequeue();

            foreach (GameObject enemy in playerLists)
            {
                if (enemy.GetComponent<PlayerCtrl>().GetPlayerName().Contains(info.id))
                {
                    int kills = info.kills;
                    int deaths= info.deaths;

                    enemy.GetComponent<PlayerCtrl>().Kills = kills;
                    enemy.GetComponent<PlayerCtrl>().Deaths = deaths;

                    break;
                }
            }
        }



        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scroeBoard.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            scroeBoard.SetActive(false);
        }
        isPrevGameStart = isGameStart;
    }

    public unsafe object ByteToStructure(Byte[] data, Type type)
    {
        IntPtr Buffer = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, Buffer, data.Length);
        object Obj = Marshal.PtrToStructure(Buffer, type);
        Marshal.FreeHGlobal(Buffer);

        if (Marshal.SizeOf(Obj) != data.Length)
        {
            return null;
        }
        return Obj;
    }

    public unsafe Byte[] StructureToByte(object obj)
    {
        int Datasize = Marshal.SizeOf(obj);
        IntPtr Buffer = Marshal.AllocHGlobal(Datasize);
        Marshal.StructureToPtr(obj, Buffer, false);
        Byte[] Data = new byte[Datasize];
        Marshal.Copy(Buffer, Data, 0, Datasize);
        Marshal.FreeHGlobal(Buffer);
        return Data;
    }

    public ConnectionManager()
    {
        this.mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        this.mNewUser = false;
        mTransBuffer = new ArrayList();
    }

    public bool ConnectionServer(string ipaddrress, int port)
    {
        if (this.mSocket.Connected)
        {
            return true;
        }
        try
        {
            //this.mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            IPAddress myIP = IPAddress.Parse(ipaddrress);
            this.mSocket.Connect(new IPEndPoint(myIP, port));

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        catch (Exception e)
        {
            string message = "서버와 연결에 실패 했습니다." + e.Message;
            Console.WriteLine(message);
            return false;
        }

        if (this.mSocket.Connected)
        {
            Thread echo_thread = new Thread(ReceiveThread);
            echo_thread.Start();
            CONNECT = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ConnectionServer(string ip)
    {
        return ConnectionServer(ip, CVSP.SpecificationCVSP.ServerPort);
    }

    public void Stop()
    {
        lock (this.mSocket)
        {
            this.mSocket.Close();
        }
        CONNECT = false;
    }

    public void EndConnectionMessage()
    {
        Send(CVSP.SpecificationCVSP.CVSP_LEAVEREQ, CVSP.SpecificationCVSP.CVSP_SUCCESS);
        Stop();
    }

    public int Send(byte cmd, byte option)
    {
        CVSP.CVSP header = new CVSP.CVSP();
        header.cmd = cmd;
        header.option = option;

        header.packetLength = (short)(4);

        byte[] buffer = new Byte[header.packetLength];
        StructureToByte(header).CopyTo(buffer, 0);
        this.mSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);

        int result = mSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);

        return result;
    }

    public void SendWithPayLoad(byte cmd, byte option, object data)
    {
        CVSP.CVSP header = new CVSP.CVSP();
        header.cmd = cmd;
        header.option = option;

        byte[] payload = StructureToByte(data);

        header.packetLength = (short)(4 + payload.Length);

        byte[] buffer = new Byte[header.packetLength];
        StructureToByte(header).CopyTo(buffer, 0);
        payload.CopyTo(buffer, 4);

        this.mSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
    }

    public void SendWithPayLoad(byte cmd, byte option, byte[] data)
    {
        CVSP.CVSP header = new CVSP.CVSP();
        header.cmd = cmd;
        header.option = option;

        //byte[] payload = StructureToByte(data);

        header.packetLength = (short)(4 + data.Length);

        byte[] buffer = new Byte[header.packetLength];
        StructureToByte(header).CopyTo(buffer, 0);
        data.CopyTo(buffer, 4);

        this.mSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
    }

    public void SendWithPayLoad(byte cmd, byte option, string message)
    {
        CVSP.CVSP header = new CVSP.CVSP();
        header.cmd = cmd;
        header.option = option;

        int euckrCodepage = 51949;

        System.Text.Encoding euckr = System.Text.Encoding.GetEncoding(euckrCodepage);

        byte[] payload = Encoding.GetEncoding("euc-kr").GetBytes(message);

        header.packetLength = (short)(4 + payload.Length);

        byte[] buffer = new Byte[header.packetLength];
        StructureToByte(header).CopyTo(buffer, 0);
        payload.CopyTo(buffer, 4);

        this.mSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
    }

    public void ReceiveThread()
    {
        int nReadBytes = 0;
        byte[] headerBuffer = new byte[4];
        byte[] payload;
        CVSP.CVSP header = new CVSP.CVSP();

        try
        {
            while (CONNECT)
            {
                nReadBytes = mSocket.Receive(headerBuffer, 4, SocketFlags.Peek);

                if (nReadBytes < 0 || nReadBytes != 4)
                {
                    continue;
                }

                header = (CVSP.CVSP)ByteToStructure(headerBuffer, header.GetType());
                nReadBytes = mSocket.Receive(mReadBuffer, header.packetLength, SocketFlags.None);
                payload = new byte[header.packetLength - headerBuffer.Length];
                Buffer.BlockCopy(mReadBuffer, headerBuffer.Length, payload, 0, header.packetLength - headerBuffer.Length);

                if (header.cmd == CVSP.SpecificationCVSP.CVSP_CHATTINGRES)
                {
                    Debug.Log("서버로 부터 받은 메시지..");
                    string message = Encoding.GetEncoding("euc-kr").GetString(payload);
                    Debug.Log("서버로부터 받은 메시지:" + message);
                    queueChat.Enqueue(message);
                }
                else if(header.cmd==CVSP.SpecificationCVSP.CVSP_JOINRES)
                {
                    if(header.option==CVSP.SpecificationCVSP.CVSP_SUCCESS)
                    {
                        isGameStart = true;
                        Debug.Log("서버에 로그인 되었습니다!");
                    }
                    else if(header.option==CVSP.SpecificationCVSP.CVSP_NEWUSER)
                    {
                        enemyName = Encoding.GetEncoding("euc-kr").GetString(payload);
                    }
                }
                else if(header.cmd==CVSP.SpecificationCVSP.CVSP_MONITORINGMSG)
                {
                    CVSP.PlayerInfo info = new CVSP.PlayerInfo();
                    info = (CVSP.PlayerInfo)ByteToStructure(payload, info.GetType());
                    queueEnemies.Enqueue(info);
                }
                else if(header.cmd==CVSP.SpecificationCVSP.CVSP_ATTACK)
                {
                    CVSP.AttackInfo info = new CVSP.AttackInfo();
                    info = (CVSP.AttackInfo)ByteToStructure(payload, info.GetType());
                    queueAttacks.Enqueue(info);
                }
                else if(header.cmd==CVSP.SpecificationCVSP.CVSP_DIE)
                {
                    CVSP.HpInfo info = new CVSP.HpInfo();
                    info = (CVSP.HpInfo)ByteToStructure(payload, info.GetType());
                    queueHp.Enqueue(info);
                }
                else if (header.cmd == CVSP.SpecificationCVSP.CVSP_SCORE)
                {
                    CVSP.ScoreInfo info = new CVSP.ScoreInfo();
                    info = (CVSP.ScoreInfo)ByteToStructure(payload, info.GetType());
                    queueScore.Enqueue(info);
                }
            }
        }
        catch (Exception e)
        {
            string temp = "에러가 발생해서 서버와의 연결을 닫습니다." + e.Message;
            Console.WriteLine(temp);
            Stop();
            CONNECT = false;
            return;
        }
    }

    private void OnDestroy()
    {
        if(CONNECT&&isGameStart)
        {
            Stop();
            isGameStart = false;
        }
    }

    IEnumerator CreatePlayer(string name)
    {
        Debug.Log("플레이어가 생성 됩니다.!");

        GameObject tempPlayer = Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity);
        tempPlayer.GetComponent<PlayerCtrl>().SetPlayerName(name);
        tempPlayer.GetComponent<PlayerCtrl>().SetInitinfo(true, this);
        playerLists.Add(tempPlayer);

        yield return null;
    }

    IEnumerator CreateEnemy(string name)
    {
        Debug.Log("적이 생성 됩니다.!");

        GameObject tempPlayer = Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity);
        tempPlayer.GetComponent<PlayerCtrl>().SetPlayerName(name);
        tempPlayer.GetComponent<PlayerCtrl>().SetInitinfo(false, this);
        playerLists.Add(tempPlayer);

        yield return null;
    }

    public void Transformation(PlayerCtrl ctrl)
    {
        if(isGameStart&&CONNECT&&isJoinRequested&&isPrevGameStart)
        {
            CVSP.PlayerInfo info = new CVSP.PlayerInfo();
            info.id = playerName;
            info.posX = ctrl.transform.position.x;
            info.posY = ctrl.transform.position.y;
            info.posZ = ctrl.transform.position.z;
            info.quatX = ctrl.transform.rotation.x;
            info.quatY = ctrl.transform.rotation.y;
            info.quatZ = ctrl.transform.rotation.z;
            info.quatW = ctrl.transform.rotation.w;

            SendWithPayLoad(CVSP.SpecificationCVSP.CVSP_OPERATIONREQ, CVSP.SpecificationCVSP.CVSP_SUCCESS, info);
        }
    }

    public void Attack(PlayerCtrl ctrl)
    {
        if (isGameStart && CONNECT && isJoinRequested && isPrevGameStart)
        {
            CVSP.AttackInfo info = new CVSP.AttackInfo();
            info.id = playerName;
            info.posX = ctrl.transform.position.x;
            info.posY = ctrl.transform.position.y;
            info.posZ = ctrl.transform.position.z;
            info.quatX = ctrl.transform.rotation.x;
            info.quatY = ctrl.transform.rotation.y;
            info.quatZ = ctrl.transform.rotation.z;
            info.quatW = ctrl.transform.rotation.w;

            SendWithPayLoad(CVSP.SpecificationCVSP.CVSP_ATTACK, CVSP.SpecificationCVSP.CVSP_SUCCESS, info);
        }
    }

    public void Hp(PlayerCtrl ctrl)
    {
        if (isGameStart && CONNECT && isJoinRequested && isPrevGameStart)
        {
            CVSP.HpInfo info = new CVSP.HpInfo();
            info.id = playerName;
            info.bulletOwner = ctrl.bulletOwner;
            info.hp = ctrl.hp;

            SendWithPayLoad(CVSP.SpecificationCVSP.CVSP_DIE, CVSP.SpecificationCVSP.CVSP_SUCCESS, info);
        }
    }

    public void Score(PlayerCtrl ctrl)
    {
        if (isGameStart && CONNECT && isJoinRequested && isPrevGameStart)
        {
            CVSP.ScoreInfo info = new CVSP.ScoreInfo();
            info.id = playerName;
            info.kills = ctrl.Kills;
            info.deaths = ctrl.Deaths;

            SendWithPayLoad(CVSP.SpecificationCVSP.CVSP_SCORE, CVSP.SpecificationCVSP.CVSP_SUCCESS, info);
        }
    }
}
    
