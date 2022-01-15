using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    private float h = 0f;
    private float v = 0f;
    private Transform tr;    
    public float speed = 10f;
    public float rotSpeed = 100f;
    public Animator animator;

    //private PhotonView pv;

    private Vector3 currPos;
    private Quaternion currRot;

    public TextMesh playerName;
    public string name = "";
    int tokenID = -1;
    bool isMine;
    ConnectionManager manager;

    public Transform firePos;
    public GameObject bullet;

    public bool isDie = false;
    public int hp = 100;
    public float respawnTime = 3.0f;

    private float time = 0.0f;

    public string bulletOwner = "";
    public int Kills=0;
    public int Deaths=0;

    private void OnTriggerEnter(Collider other)
    {
        if(this.isMine)
        {
            Debug.Log("Hit!:" + other.gameObject.tag);
            if (other.gameObject.tag == "BULLET")
            {
                Debug.Log("owner!:" + other.gameObject.GetComponent<Bullet>().owner + "target:" + name);
                if (other.gameObject.GetComponent<Bullet>().owner == name)
                {
                    return;
                }

                if (isDie == true)
                {
                    return;
                }

                Debug.Log("Hit");
                Destroy(other.gameObject);
                hp -= 10;
                bulletOwner = other.gameObject.GetComponent<Bullet>().owner;
                manager.SendMessage("Hp", this);

                animator.SetTrigger("Hit");

                if (hp <= 0)
                {
                    animator.SetTrigger("Die");
                    StartCoroutine(RespawnPlayer(respawnTime));
                }

                
            }
        }
       
    }

    IEnumerator RespawnPlayer(float waitTime)
    {
        if(this.isMine)
        {
            Deaths += 1;
            manager.SendMessage("Score", this);
        }        
        Debug.Log("Died!");
        isDie = true;
        StartCoroutine(PlayerVisible(false, 0.5f));
        yield return new WaitForSeconds(waitTime);

        if(this.isMine)
        {
            tr.position = new Vector3(Random.Range(-20.0f, 20.0f), 0.0f, Random.Range(-20.0f, 20.0f));
        }        

        hp = 100;
        isDie = false;
        animator.SetTrigger("Reset");
        StartCoroutine(PlayerVisible(true, 1.0f));
    }

    [PunRPC]
    public void RespawnRPC()
    {
        StartCoroutine(RespawnPlayer(respawnTime));
    }

    IEnumerator PlayerVisible(bool visibled, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        GetComponentInChildren<MeshRenderer>().enabled = visibled;
    }

    IEnumerator CreateBullet()
    {
        GameObject bulletObject = Instantiate(bullet, firePos.position, firePos.rotation);
        bulletObject.GetComponent<Bullet>().owner = name;
        yield return null;
    }

    void Fire()
    {
        StartCoroutine(CreateBullet());
        manager.SendMessage("Attack", this);
        //pv.RPC("FireRPC", PhotonTargets.Others);
    }

    [PunRPC]
    public void FireRPC()
    {
        StartCoroutine(CreateBullet());
    }

    // Start is called before the first frame update
    void Start()
    {
        tr = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        //pv = GetComponent<PhotonView>();

        //pv.ObservedComponents[0] = this;

        //if(pv.isMine)
        //{
        //    Debug.Log(Camera.main);
        //    Camera.main.GetComponent<FollowCam>().targetTr = tr.Find("Cube").gameObject.transform;
        //}
        
    }

    public void SetInitinfo(bool isMine,ConnectionManager manager)
    {
        this.isMine = isMine;
        this.manager = manager;

        if (this.isMine)
        {
            Camera.main.GetComponent<FollowCam>().targetTr = this.transform.Find("Cube").gameObject.transform;
        }
    }
   
    // Update is called once per frame
    void Update()
    {
        if(this.isMine&&isDie==false)
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
            tr.Translate(moveDir.normalized*Time.deltaTime*speed);
            tr.Rotate(Vector3.up * Time.deltaTime * rotSpeed*Input.GetAxis("Mouse X"));

            if(moveDir.magnitude>0)
            {
                animator.SetFloat("Speed", 1.0f);
            }
            else
            {
                animator.SetFloat("Speed", 0.0f);
            }

            this.time += Time.deltaTime;

            if (this.time > 0.01f)
            {
                //this.manager.Transformation(this);
                manager.SendMessage("Transformation", this);
                this.time = 0;
            }

            if (Input.GetButtonDown("Fire1"))
            {
                animator.SetTrigger("Attack");
                Fire();
            }

        }
        //else if (this.isMine==false)
        //{
        //    if (tr.position != currPos)
        //    {
        //        animator.SetFloat("Speed", 1.0f);
        //    }
        //    else
        //    {
        //        animator.SetFloat("Speed", 0.0f);
        //    }
        //    tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * 10.0f);
        //    tr.rotation = Quaternion.Lerp(tr.rotation, currRot, 0.01f * 10.0f);
        //}

        ////if(pv.isMine&&isDie==false)
        //{
        //    h = Input.GetAxis("Horizontal");
        //    v = Input.GetAxis("Vertical");

        //    Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        //    tr.Translate(moveDir.normalized * Time.deltaTime * speed);
        //    tr.Rotate(Vector3.up * Time.deltaTime * rotSpeed * Input.GetAxis("Mouse X"));

        //    if (moveDir.magnitude > 0)
        //    {
        //        animator.SetFloat("Speed", 1.0f);
        //    }
        //    else
        //    {
        //        animator.SetFloat("Speed", 0.0f);
        //    }

        //    if(Input.GetButtonDown("Fire1"))
        //    {
        //        animator.SetTrigger("Attack");
        //        Fire();
        //    }
        //}
        ////else// if(!pv.isMine)
        //{
        //    if(tr.position!=currPos)
        //    {
        //        animator.SetFloat("Speed", 1.0f);
        //    }
        //    else
        //    {
        //        animator.SetFloat("Speed", 0.0f);
        //    }
        //    tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * 10.0f);
        //    //tr.rotation = Quaternion.Lerp(tr.rotation, currRot, Time.deltaTime * 10.0f);Lerp 클램프는 0과 1 사이의 클램프로 고정한다. Time.deltaTime을 사용할경우 1을벗어나 오류남
        //    tr.rotation = Quaternion.Lerp(tr.rotation, currRot, 0.01f * 10.0f);
        //}
    }

    //void OnPhotonSerializeView(PhotonStream stream,PhotonMessageInfo info)
    //{
    //    if(stream.isWriting)
    //    {
    //        stream.SendNext(tr.position);
    //        stream.SendNext(tr.rotation);
    //        stream.SendNext(name);
    //    }
    //    else
    //    {
    //        currPos = (Vector3)stream.ReceiveNext();
    //        currRot = (Quaternion)stream.ReceiveNext();
    //        SetPlayerName((string)stream.ReceiveNext());
    //    }
    //}

    public void SetPlayerName(string name)
    {
        this.name = name;
        GetComponent<PlayerCtrl>().playerName.text = this.name;
    }

    public string GetPlayerName()
    {
        return this.name;
    }
}
