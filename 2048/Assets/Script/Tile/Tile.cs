﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public bool mergeThisTurn = false;
    public int indRow;
    public int indCol;
    // TileAround untuk menyimpan tile disekitarnya untuk bantu mendapatkan input layer, array size 4 yaitu left, up, right, down
    public Tile[] TileAround = new Tile[4];
    public int Number
    {
        get
        {
            return number;
        }
        set
        {
            number = value;
            if (number == 0)
            {
                SetEmpty();
            }
            else
            {
                ApplyStyle(number);
                SetVisible();
            }
        }
    } 
    private int number;
    private Text TileText;
    private Image TileImage;
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        TileText = GetComponentInChildren<Text>();
        TileImage = transform.Find("NumberedCell").GetComponent<Image>();
    }

    public void PlayMergeAnimation()
    {
        anim.SetTrigger("Merge");
    }
    public void PlayAppearAnimation()
    {
        anim.SetTrigger("Appear");
    }

    void ApplyStyleFromHolder(int index)
    {
        TileText.text = TileStyleHolder.Instance.tileStyles[index].number.ToString();
        TileText.color = TileStyleHolder.Instance.tileStyles[index].text_color;
        TileImage.color = TileStyleHolder.Instance.tileStyles[index].tile_color;
    }

    void ApplyStyle(int num)
    {
        switch (num)
        {
            case 2:
                ApplyStyleFromHolder(0);
                break;
            case 4:
                ApplyStyleFromHolder(1);
                break;
            case 8:
                ApplyStyleFromHolder(2);
                break;
            case 16:
                ApplyStyleFromHolder(3);
                break;
            case 32:
                ApplyStyleFromHolder(4);
                break;
            case 64:
                ApplyStyleFromHolder(5);
                break;
            case 128:
                ApplyStyleFromHolder(6);
                break;
            case 256:
                ApplyStyleFromHolder(7);
                break;
            case 512:
                ApplyStyleFromHolder(8);
                break;
            case 1024:
                ApplyStyleFromHolder(9);
                break;
            case 2048:
                ApplyStyleFromHolder(10);
                break;
            case 4096:
                ApplyStyleFromHolder(11);
                break;
            case 8192:
                ApplyStyleFromHolder(12);
                break;
            case 16384:
                ApplyStyleFromHolder(13);
                break;
            case 32768:
                ApplyStyleFromHolder(14);
                break;
            default:
                Debug.Log("Error check number you pass in here");
                break;
        }
    }

    private void SetVisible()
    {
        TileImage.enabled = true;
        TileText.enabled = true;
    }

    private void SetEmpty()
    {
        TileImage.enabled = false;
        TileText.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
