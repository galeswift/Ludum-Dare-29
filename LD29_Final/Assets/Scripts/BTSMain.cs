using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EBrickTypes
{
	BRICK_RED,
	BRICK_YELLOW,
	BRICK_GREEN,
    BRICK_BLUE,
    BRICK_SPECIAL,
	COUNT,
}

public class PlayerPiece
{
    public Brick[] bricks = new Brick[2];
    public float startRadius = 4.0f;
    public BTSMain m_owner;
    public float curInnerRadius;
    public float moveTimer;
     
    public float rotationVel=0.0f;
    public float maxVel = 200.0f;
    public float rotationAccel=200.0f;
    public float rotationBraking=400.0f;
    public float smoothRotation = 6.0f;
    public float repeatDelay=0.15f;    
    public float dropTimer=0.5f;  
    public float timeSinceKeyDown=0.0f;
    public float desiredRotation=0;
    private bool dropped = false;
    private bool valid = true;
    public PlayerPiece(BTSMain owner)
    {
        m_owner = owner;
        dropTimer = 0.5f - (.2f * m_owner.level / 10.0f);
        if (dropTimer <= 0.2f)
        {
            dropTimer = 0.2f;
        }
    
        bool createdSpecialGem = false;
        for( int i=0 ; i<2 ; i++ )
        {
            Vector2 newPos = new Vector2(owner.numLayers-2,i);
            if( owner.CanMoveBrick(newPos) )
            {
                bool isCrashGem = m_owner.crashGemPercent >= Random.value;
                bool isSpecialGem = m_owner.specialGemPercent >= Random.value;
                if( owner.CenterIsExposed() )
                {
                    bricks[i] = owner.CreateBrick((int)newPos.x, (int)newPos.y, null, true, true, owner.centerType);
                }
                else if( isSpecialGem && !createdSpecialGem)
                {
                    createdSpecialGem=true;
                    bricks[i] = owner.CreateBrick((int)newPos.x, (int)newPos.y, null, true, true, EBrickTypes.BRICK_SPECIAL);
                }
                else
                {
                    bricks[i] = owner.CreateBrick((int)newPos.x, (int)newPos.y, null, isCrashGem);
                }
            }
            else
            {
                valid = false;
            }
        }
        if ( valid && !__Rotate(1))
        {
            valid = false;
        }
    }
    public void Reset()
    {
        for (int i=0; i<2; i++)
        {
            if( bricks[i] != null )
            {
                GameObject.Destroy(bricks[i].gameObject);
            }
        }
    }

    public void Update(float dt)
    {
        __UpdateDrop(dt);
        __UpdateRotate(dt);
        __UpdateMove(dt);
        __UpdateForceDrop(dt);
    }

    public bool IsInValidPos()
    {
        return valid;
    }
    void __UpdateForceDrop(float dt)
    {
        if( Input.GetKeyDown(KeyCode.Space) )
        {
            bool canMove=true;
            while(canMove)
            {
                Vector2 moveDir = new Vector2(-1, 0);
               
                for( int i=0 ; i<2 ; i++ )
                {
                    if( bricks[i] != null )
                    {
                        Vector2 newPos = bricks[i].pos + moveDir;
                        if( newPos.y >= m_owner.numBricksPerLayer )
                        {
                            newPos.y = 0;
                        }
                        else if( newPos.y < 0 )
                        {
                            newPos.y = m_owner.numBricksPerLayer-1;
                        }
                        if( !m_owner.CanMoveBrick(newPos) )
                        {
                            canMove = false;
                            break;
                        }
                    }
                    else
                    {
                        canMove = false;
                    }
                }
                
                if( canMove )
                {
                    AudioSource.PlayClipAtPoint(m_owner.soundFX[(int)BTSMain.SoundEffectType.SFX_MOVE],Camera.main.transform.position,0.3f);
                    for( int i=0 ; i<2 ; i++ )
                    {
                        Vector2 newPos = bricks[i].pos + moveDir;
                        if( newPos.y >= m_owner.numBricksPerLayer )
                        {
                            newPos.y = 0;
                        }
                        else if( newPos.y < 0 )
                        {
                            newPos.y = m_owner.numBricksPerLayer-1;
                        }
                        
                        bricks[i].pos = newPos;
                    }
                }
            }
        }
        __UpdateDrop(dt);
    }
    void __UpdateDrop(float dt)
    {
        if (dropped)
        {
            return;
        }
        moveTimer += dt;

        if (moveTimer > dropTimer)
        {
            moveTimer=0.0f;

            bool shouldDrop = false;    

            for( int i=0 ; i<2 ; i++ )
            {
                Vector2 newPos = new Vector2(bricks[i].pos.x -1, bricks[i].pos.y);

                if( !m_owner.CanMoveBrick(newPos) )
                {
                    shouldDrop = true;
                }           
            }


            if( shouldDrop )
            {
                AudioSource.PlayClipAtPoint(m_owner.soundFX[(int)BTSMain.SoundEffectType.SFX_DROP],Camera.main.transform.position,0.3f);
                for( int i=0 ; i<2 ; i++ )
                {
                    dropped = true;
                    bricks[i].transform.parent = m_owner.centerObject.transform;
                    m_owner.MoveBrick(bricks[i].pos, bricks[i]);
                    bricks[i] = null;
                }
            }
            else
            {         
                for( int i=0 ; i<2 ; i++ )
                {
                    Vector2 newPos = new Vector2(bricks[i].pos.x -1, bricks[i].pos.y);
                    bricks[i].pos = newPos;
                }
            }
        } 
    }

    private int rotation=0;
    void __UpdateRotate(float dt)
    {
        if (dropped)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            __Rotate(1);           
        }
    }

    bool __Rotate(int dir)
    {
        bool result = true;
        rotation += dir;
        
        if( rotation >3 )
        {
            rotation =0;
        }
        
        Vector2 desiredPos = new Vector2();
        switch(rotation)
        {
            case 0:
                desiredPos = new Vector2(bricks[0].pos.x, bricks[0].pos.y+1);
                break;
            case 1:
                desiredPos = new Vector2(bricks[0].pos.x+1, bricks[0].pos.y);
                break;
            case 2:
                desiredPos = new Vector2(bricks[0].pos.x, bricks[0].pos.y-1);
                break;
            case 3:
                desiredPos = new Vector2(bricks[0].pos.x-1, bricks[0].pos.y);
                break;
        }
        
        if( desiredPos.y > m_owner.numBricksPerLayer-1 ) 
        {
            desiredPos.y = 0;
        }
        else if( desiredPos.y < 0 )
        {
            desiredPos.y = m_owner.numBricksPerLayer-1;
        }
        if( m_owner.CanMoveBrick(desiredPos) )
        {
            bricks[1].pos = desiredPos;
        }
        else
        {
            result = false;
            rotation--;
            if( rotation < 0 )
            {
                rotation = 3;
            }
        }

        return result;
    }

    void __UpdateMove(float dt)
    {
        if (dropped)
        {
            return;
        }

        bool keyIsDown = false;
        if (Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.DownArrow))
        {           
            keyIsDown = true;
        } 

        
        if (keyIsDown)
        {
            timeSinceKeyDown += Time.deltaTime;
            
            if (timeSinceKeyDown >= repeatDelay)
            {
                timeSinceKeyDown = 0.0f;

                bool canMove=true;
                int newCol = Input.GetKey(KeyCode.RightArrow) ? -1 : Input.GetKey(KeyCode.LeftArrow) ? 1 : 0;
                Vector2 moveDir = new Vector2( Input.GetKey(KeyCode.DownArrow) ? -1 : 0 , newCol);

                for( int i=0 ; i<2 ; i++ )
                {
                    Vector2 newPos = bricks[i].pos + moveDir;
                    if( newPos.y >= m_owner.numBricksPerLayer )
                    {
                        newPos.y = 0;
                    }
                    else if( newPos.y < 0 )
                    {
                        newPos.y = m_owner.numBricksPerLayer-1;
                    }
                    if( !m_owner.CanMoveBrick(newPos) )
                    {
                        canMove = false;
                        break;
                    }
                }

                if( canMove )
                {
                    AudioSource.PlayClipAtPoint(m_owner.soundFX[(int)BTSMain.SoundEffectType.SFX_MOVE],Camera.main.transform.position,0.3f);
                    for( int i=0 ; i<2 ; i++ )
                    {
                        Vector2 newPos = bricks[i].pos + moveDir;
                        if( newPos.y >= m_owner.numBricksPerLayer )
                        {
                            newPos.y = 0;
                        }
                        else if( newPos.y < 0 )
                        {
                            newPos.y = m_owner.numBricksPerLayer-1;
                        }

                        bricks[i].pos = newPos;
                    }
                }
            }
        } 
        else
        {
            timeSinceKeyDown = repeatDelay;
        }     
    }
    public bool NeedsNewBrick()
    {        
        // If both bricks are null, we're done
        for (int i=0; i<2; i++)
        {
            if( bricks[i] != null )
            {
                return false;
            }
        }

        return true;
    }
}

public class BTSMain : MonoBehaviour
{
    public enum GemState
    {
        STATE_PLAYERMOVING,
        STATE_DROPPING_PLAYER,
        STATE_BREAKING,
        STATE_DROPPING,
        STATE_GAMEOVER,
        STATE_VICTORY,
    }

    public enum SoundEffectType
    {
        SFX_MOVE,
        SFX_DROP,
        SFX_BLOWUP,
        SFX_COUNT,
    }
    public AudioClip[] soundFX = new AudioClip[(int)SoundEffectType.SFX_COUNT];
  	public Material[] materials = new Material[(int)EBrickTypes.COUNT];
    public Material[] crashGemMaterials = new Material[(int)EBrickTypes.COUNT];
    public Material wallMaterial;
    public GemState state;
	public GameObject brickPrefab;
	public GameObject centerObject;
    public EBrickTypes centerType;
    public Brick wallBrick;
    public GameObject levelText;
    public GameObject scoreText;
    public GameObject gameOverText;
    public GameObject victoryText;
    public GameObject centerExplosionTemplate;
	public PlayerPiece playerPiece; 
    public float levelRestartTime = 3.0f;
    public float timeUntilReset=0.0f;
    public int numInitialLayers=2;
    public int numLayers = 3;
	public int numBricksPerLayer = 10;	
    public int score = 0;
    public float scoreMultiplier=1.0f;
	public float brickHeight = 0.5f;
	public float startInnerRadius = 1.0f;
    public float timeSinceBreakCheck=0.0f;
    public float breakDelay = 0.3f;
    public float breakDelayDefault = 0.3f;
    public float brickWidth;	
    public float crashGemPercent = 0.5f;
    public float specialGemPercent = 0.1f;
    public Brick [,] bricks;
    public int level = 1;
    public bool pendingVictory = false;
    private bool isBreakingGems;
    public Brick CreateBrick(int row, int col, GameObject center, bool crashGem, bool wantsOverride=false, EBrickTypes overrideType=EBrickTypes.COUNT )
    {
        GameObject newBrick = (GameObject)Instantiate(brickPrefab);
        
        Brick brickComp = newBrick.GetComponent<Brick>();
        brickComp.width = brickWidth;
        brickComp.height = brickHeight;

        // No special gems regularly
        brickComp.type = wantsOverride ? overrideType : (EBrickTypes)Random.Range((int)EBrickTypes.BRICK_RED, (int)EBrickTypes.BRICK_SPECIAL);
        if (crashGem)
        {           
            brickComp.newMaterial = crashGemMaterials[(int)brickComp.type];
        }
        else
        {
            brickComp.newMaterial = materials[(int)brickComp.type];;
        }
        //brickComp.newMaterial = materials[(int)brickComp.type];
        if (center != null)
        {
            newBrick.transform.parent = center.transform;
            newBrick.transform.position = new Vector3(0, 0, 0);
        }
        brickComp.isCrash = crashGem;
        brickComp.pos = new Vector2(row, col);
        return brickComp;

    }

    void InitBricks()
    {
        // Init the center
        centerType = (EBrickTypes)Random.Range((int)EBrickTypes.BRICK_RED, (int)EBrickTypes.COUNT-1);
        switch (centerType)
        {
            case EBrickTypes.BRICK_RED:
                centerObject.transform.GetComponent<Renderer>().material.color = Color.red;
                break;  
            case EBrickTypes.BRICK_GREEN:
                centerObject.transform.GetComponent<Renderer>().material.color = Color.green;
                break;
              case EBrickTypes.BRICK_BLUE:
                centerObject.transform.GetComponent<Renderer>().material.color = Color.blue;
                break;
            case EBrickTypes.BRICK_YELLOW:            
                centerObject.transform.GetComponent<Renderer>().material.color = Color.yellow;
                break;
                
        }

        bricks = new Brick[numLayers, numBricksPerLayer];
        brickWidth = 360.0f / numBricksPerLayer;

        List<EBrickTypes> validRandomTypes = new List<EBrickTypes>();

        for (int i=0; i<(int)EBrickTypes.COUNT-1; i++)
        {
            if( (EBrickTypes)i != centerType )
            {
                validRandomTypes.Add((EBrickTypes)i);
            }
        }

        for (int i = 0; i < numInitialLayers; i++)
        {
            bool wantsOverride = (i==0 || i%3==0);
            EBrickTypes overrideType = EBrickTypes.COUNT;
            if( wantsOverride )
            {
                int overrideIndex = Random.Range(0, validRandomTypes.Count);
                overrideType = validRandomTypes[overrideIndex];
                validRandomTypes.RemoveAt(overrideIndex);
            }

            if( validRandomTypes.Count == 0 )
            {
                for (int j=0; j<(int)EBrickTypes.COUNT-1; j++)
                {
                    if( (EBrickTypes)j != centerType )
                    {
                        validRandomTypes.Add((EBrickTypes)j);
                    }
                }
            }

            for (int j = 0; j < numBricksPerLayer; j++)
            {
                bricks[i,j] = CreateBrick(i,j, centerObject, false, wantsOverride, overrideType);
            }
        }

            wallBrick = CreateBrick(numLayers-1, 0, centerObject, false);
            wallBrick.width = 360.0f;
            wallBrick.BuildMesh(true);
            wallBrick.transform.GetComponent<Renderer>().material = wallMaterial;
    
    }

    public bool CanMoveBrick( Vector2 newPos )
    {
        bool result = false;
        if (newPos.x >= 0 && 
            newPos.x < numLayers &&
            newPos.y >= 0 && newPos.y < numBricksPerLayer)
        {
            Brick destBrick = bricks [(int)newPos.x, (int)newPos.y];
            result = destBrick == null;
        }
        return result;
    }
    public void MoveBrick( Vector2 newPos, Brick brick )
    {   
        bricks [(int)brick.pos.x, (int)brick.pos.y] = null;
        brick.pos = newPos;
        bricks [(int)newPos.x, (int)newPos.y] = brick;
    }

	// Use this for initialization
	void Start()
	{
        pendingVictory = false;
        centerObject.SetActive(true);
        numInitialLayers = level;
        numLayers = level + 14;
        numBricksPerLayer = 6 + (int)(level / 5);
        Camera.main.orthographicSize = 6 + level/2.0f;
        InitBricks();
        CreatePlayerPiece();
	}

    public bool CenterIsExposed()
    {
        for( int i=0 ; i<numBricksPerLayer; i++ )
        {
            if( bricks[0,i] == null )
            {
                return true;
            }
        }
        return false;
    }
    void CreatePlayerPiece()
    {
        playerPiece = new PlayerPiece(this);
        if (playerPiece.IsInValidPos())
        {
            state = GemState.STATE_PLAYERMOVING;
        } 
        else
        {
            GameOver();
        }
    }

    bool BreakGems()
    {
        bool result = false;
        timeSinceBreakCheck += Time.deltaTime;
        if (timeSinceBreakCheck >= breakDelay)
        {
            timeSinceBreakCheck = 0.0f;
        
            for (int i=0; i<numLayers; i++)
            {
                for (int j=0; j<numBricksPerLayer; j++)
                {
                    if( bricks[i,j] != null &&
                        (bricks[i,j].isCrash ||
                       bricks[i,j].isBreaking))
                    {
                        TriggerBreak(bricks [i, j]);
                    }
                }
            }

            for (int i=0; i<numLayers; i++)
            {
                for (int j=0; j<numBricksPerLayer; j++)
                {
                    if (bricks [i, j] != null &&
                        bricks [i, j].isPendingBreak)
                    {
                        bricks[i,j].Break();
                        NotifyBreakSingleGem();
                        result = true;
                    }
                }
            }

            if( result )
            {
                breakDelay *= 0.9f;
            }
        } 
        else
        {        
            result = true;
        }

        return result;
    }

    void NotifyBreakSingleGem()
    {
        AudioSource.PlayClipAtPoint(soundFX[(int)SoundEffectType.SFX_BLOWUP],Camera.main.transform.position,0.3f);
        scoreMultiplier += 0.2f;
        score += (int)(scoreMultiplier * 10);
        if (scoreMultiplier > 2.0f)
        {
            Camera.main.GetComponent<CameraShake>().Shake(4.0f, 0.01f);
        }
    }

    void TriggerBreak( Brick brick )
    {     
        // look at neighbors
        Vector2 left, up, down, right;
        left = brick.pos;
        left.y--;
        if (left.y < 0)
            left.y = numBricksPerLayer - 1;

        right = brick.pos;
        right.y++;
        if (right.y >= numBricksPerLayer)
            right.y = 0;

        // Check left
        Brick leftBrick = bricks [(int)left.x, (int)left.y];
        if (leftBrick != null &&
            leftBrick.type == brick.type)
        {
            brick.SetPendingBreak();
            leftBrick.SetPendingBreak();
        }

        // check right
        Brick rightBrick = bricks [(int)right.x, (int)right.y];
        if (rightBrick != null &&
            rightBrick.type == brick.type)
        {
            brick.SetPendingBreak();
            rightBrick.SetPendingBreak();
        }

        if (brick.pos.x < numLayers - 2)
        {
            up = brick.pos;
            up.x++;
            // Check above
            Brick upBrick = bricks [(int)up.x, (int)up.y];
            if (upBrick != null &&
                upBrick.type == brick.type)
            {
                brick.SetPendingBreak();
                upBrick.SetPendingBreak();
            }
        }

        if (brick.pos.x > 0)
        {
            // check below
            down = brick.pos;
            down.x--;
            Brick downBrick = bricks [(int)down.x, (int)down.y];
            if( downBrick != null &&
               brick.type == EBrickTypes.BRICK_SPECIAL )
            {
                brick.SetPendingBreak();
                SpecialDrop(downBrick.type);
            }
            else if (downBrick != null &&
                downBrick.type == brick.type)
            {
                brick.SetPendingBreak();
                downBrick.SetPendingBreak();
            }
        } else if (brick.pos.x == 0 &&
            brick.type == centerType)
        {
            Instantiate(centerExplosionTemplate,centerObject.transform.position, centerObject.transform.rotation);
            centerObject.SetActive(false);
            pendingVictory=true;
        }
    }

    void SpecialDrop(EBrickTypes breakType)
    {
        Camera.main.GetComponent<CameraShake>().Shake(3.0f, 0.1f);
        for (int i=0; i<numLayers; i++)
        {
            for (int j=0; j<numBricksPerLayer; j++)
            {
                if( bricks[i,j] != null &&
                   bricks[i,j].type == breakType)
                {
                    bricks[i,j].SetPendingBreak();
                }
            }
        }
    }

    bool CleanupBrokenGems()
    {
        bool result = true;
        for (int i=0; i<numLayers; i++)
        {
            for (int j=0; j<numBricksPerLayer; j++)
            {
                if( bricks[i,j] != null &&
                   bricks[i,j].isBreaking )
                {
                    DestroyObject( bricks[i,j].gameObject );
                    bricks[i,j] = null;
                    result= false;
                }
            }
        }
        if (result)
        {
            scoreMultiplier=1.0f;
            breakDelay = breakDelayDefault;
        }
        return result;
    }      

    bool DropGems()
    {
        bool result = false;
        for (int i=0; i<numLayers; i++)
        {
            for (int j=0; j<numBricksPerLayer; j++)
            {
                Brick curBrick = bricks[i,j];
                while( curBrick != null &&
                    CanMoveBrick(new Vector2(curBrick.pos.x-1, curBrick.pos.y)))
                {
                    MoveBrick(new Vector2(curBrick.pos.x-1, curBrick.pos.y), curBrick);
                    result = true;
                }
            }
        }
        return result;
    }

    void GameOver()
    {
        state = GemState.STATE_GAMEOVER;
        gameOverText.SetActive(true);
        timeUntilReset = levelRestartTime;
    }

    void Victory()
    {
        Camera.main.GetComponent<CameraShake>().Shake(3.0f, 0.1f);
        state = GemState.STATE_VICTORY;
        victoryText.SetActive(true);
        timeUntilReset = levelRestartTime;
    }

    void Reset()
    {
        for (int i=0; i<numLayers; i++)
        {
            for( int j=0 ; j<numBricksPerLayer ; j++ )
            {
                if( bricks[i,j] != null )
                {
                    Destroy(bricks[i,j].gameObject);
                    bricks[i,j] = null;
                }
            }
        }
        if (playerPiece != null)
        {
            playerPiece.Reset();
            playerPiece = null;
        }
        Destroy(wallBrick.gameObject);

        if (state == GemState.STATE_VICTORY)
        {
            level++;
        }
       
        victoryText.SetActive(false);
        gameOverText.SetActive(false);
        Start();
    }

	// Update is called once per frame
	void Update()
	{
        GUIText textLabel = levelText.GetComponent<GUIText>();
        textLabel.text = string.Format("{0:000000000}", level);
        GUIText scoreLabel = scoreText.GetComponent<GUIText>();
        scoreLabel.text = string.Format("{0:000000000}", score);
        if (state == GemState.STATE_GAMEOVER ||
            state == GemState.STATE_VICTORY)
        {
            if( Input.GetKeyDown(KeyCode.Space) )        
            {
                timeUntilReset=0;
            }
            if( timeUntilReset <= 0)
            {
                Reset();
            }
            return;
        }

        if (playerPiece != null)
        {
            playerPiece.Update(Time.deltaTime);

            
            // Player dropped a brick
            if (playerPiece.NeedsNewBrick())
            {
                state = GemState.STATE_DROPPING_PLAYER;
            }
        }

        if (state == GemState.STATE_DROPPING_PLAYER)
        {
            DropGems();
            state = GemState.STATE_BREAKING;
        }

        if( state == GemState.STATE_BREAKING )
        {
            if( BreakGems() )
            {
                state = GemState.STATE_BREAKING;
            }
            else
            {
                CleanupBrokenGems();
                state = GemState.STATE_DROPPING;    
            }
        }   

        if (pendingVictory)
        {
            Victory();
        }
        if (state == GemState.STATE_DROPPING)
        {
            if( DropGems() )
            {
                state = GemState.STATE_BREAKING;
            }
            else
            {
                CreatePlayerPiece();
            }
        }               
	}
}
