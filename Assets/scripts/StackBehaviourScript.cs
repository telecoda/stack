using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;


public class StackBehaviourScript : MonoBehaviour {

	private const string PLAYING = "playing";
	private const string GAME_OVER = "gameover";
		
	private const float BLOCK_WIDTH = 1f;
	private const float BLOCK_HEIGHT = 0.25f;
	private const float BLOCK_BOUNDS = 1.5f;

	private string state;
	private int blockCount =1;

	// GameObjects
	private GameObject topBlock;
	private GameObject movingBlock;
	private bool movingBlockXDir;
	private Camera theCamera;
	public UnityEngine.UI.Text scoreLabel;

	private int score;

	private float tileSpeed =1.5f;
	private float tileTransition = 0.0f;

	private bool blockAdded;

	void Start () {
	
		score = 0;
		state = PLAYING;

		// default topBlock to cube in stack
		topBlock = transform.gameObject;

		// set Colour of first block
		topBlock.GetComponent<Renderer>().material.color = new Color(1,0,0);		
		movingBlockXDir = true;

		// get important refs
		theCamera = Camera.main;

		// init moving block
		NewMovingBlock();

	}
			
	// Update is called once per frame
	void Update () {

		switch (state) {
		case PLAYING:
			{
				playingUpdate ();
				break;
			}
		case GAME_OVER:
			{
				gameOverUpdate ();
				break;
			}
		}

	}

	void gameOverUpdate() {
	}

	void playingUpdate() {

		if (Input.anyKeyDown) {
			blockAdded = AddCube ();
			if (!blockAdded) {
				state = GAME_OVER;
				Debug.Log ("GAME OVER!");
			} else {
				blockCount++;
				score++;
				scoreLabel.text = score.ToString ();
			}
			tileTransition = 0;
		} else {
			tileTransition += Time.deltaTime * tileSpeed;
			if (movingBlockXDir) {
				// slide block on x axis
				movingBlock.transform.position = new Vector3 (Mathf.Sin (tileTransition) * BLOCK_BOUNDS, movingBlock.transform.position.y, movingBlock.transform.position.z);
			} else {
				// slide block on z axis
				movingBlock.transform.position = new Vector3 (movingBlock.transform.position.x,movingBlock.transform.position.y,Mathf.Sin (tileTransition) * BLOCK_BOUNDS);
			} 
		}
	}

	bool AddCube() {

		// copy moving block to top of stack

		// calc dimensions of block to add based on size of the overlap between moving block and top block

		Vector3 tPos = topBlock.transform.position;
		Vector3 tScale = topBlock.transform.localScale;

		Vector3 mPos = movingBlock.transform.position;// + movingBlock.transform.localPosition;
		Vector3 mScale = movingBlock.transform.localScale;

		// calc min and max x,z co-ords for both cubes
		float tMinX = tPos.x - tScale.x/2;
		float tMaxX = tPos.x + tScale.x/2;
		float tMinZ = tPos.z - tScale.z/2;
		float tMaxZ = tPos.z + tScale.z/2;

		float mMinX = mPos.x - mScale.x/2;
		float mMaxX = mPos.x + mScale.x/2;
		float mMinZ = mPos.z - mScale.z/2;
		float mMaxZ = mPos.z + mScale.z/2;

		// use the LARGEST min and the SMALLEST max values
		// for the resulting cube

		float rMinX, rMaxX, rMinZ, rMaxZ;

		if (tMinX >= mMinX) {
			rMinX = tMinX;
		} else {
			rMinX = mMinX;
		}
		if (tMaxX >= mMaxX) {
			rMaxX = mMaxX;
		} else {
			rMaxX = tMaxX;
		}

		if (tMinZ >= mMinZ) {
			rMinZ = tMinZ;
		} else {
			rMinZ = mMinZ;
		}
		if (tMaxZ >= mMaxZ) {
			rMaxZ = mMaxZ;
		} else {
			rMaxZ = tMaxZ;
		}

//		Debug.Log ("===========================================");
//		Debug.LogFormat ("Adding block {0}", blockCount + 1);
//		Debug.Log ("===========================================");
//		Debug.LogFormat ("mPos: {0} mScale: {1}", mPos, mScale);
//		Debug.LogFormat ("mMinX: {0} mMaxX: {1}", mMinX, mMaxX);
//		Debug.LogFormat ("mMinZ: {0} mMaxZ: {1}", mMinZ, mMaxZ);
//		Debug.LogFormat ("tPos: {0} tScale: {1}", tPos, tScale);
//		Debug.LogFormat ("tMinX: {0} tMaxX: {1}", tMinX, tMaxX);
//		Debug.LogFormat ("tMinZ: {0} tMaxZ: {1}", tMinZ, tMaxZ);
//		Debug.LogFormat ("rMinX: {0} rMaxX: {1}", rMinX, rMaxX);
//		Debug.LogFormat ("rMinZ: {0} rMaxZ: {1}", rMinZ, rMaxZ);
//
		// scale movingBlock
		float xWidth = rMaxX-rMinX;
		float zWidth = rMaxZ-rMinZ;

		if (xWidth < 0 || zWidth < 0) {
			// Game over
			return false;
		}
			
		// centre new block, based on intersection of movingBlock and topBlock
		float centreX = movingBlock.transform.position.x;
		float centreZ = movingBlock.transform.position.z; 

		// calc size/pos of broken slice
		float bxWidth = mScale.x; 
		float bzWidth = mScale.z; 

		if (movingBlockXDir) {
			if (mMaxX >= tMaxX) {
				// moving block is further than top block
				centreX = (tMaxX - xWidth/2);

				bxWidth = mMaxX - tMaxX;
				float bxPos = centreX+xWidth/2+bxWidth/2;
				NewBrokenBlock(bxPos,mPos.y,mPos.z,bxWidth,BLOCK_HEIGHT,bzWidth);
			} else {
				// moving block is nearer than top block
				centreX = (mMaxX - xWidth/2);

				bxWidth = tMinX - mMinX;
				float bxPos = centreX-xWidth/2-bxWidth/2;
				NewBrokenBlock(bxPos,mPos.y,mPos.z,bxWidth,BLOCK_HEIGHT,bzWidth);
			}
		} else {
			if (mMaxZ >= tMaxZ) {
				// moving block is further than top block
				centreZ = (tMaxZ - zWidth/2);

				bzWidth = mMaxZ - tMaxZ;
				float bzPos = centreZ+zWidth/2+bzWidth/2;
				NewBrokenBlock(mPos.x,mPos.y,bzPos,bxWidth,BLOCK_HEIGHT,bzWidth);
			} else {
				// moving block is nearer than top block
				centreZ = (mMaxZ - zWidth/2);

				bzWidth = tMinZ - mMinZ;
				float bzPos = centreZ-zWidth/2-bzWidth/2;
				NewBrokenBlock(mPos.z,mPos.y,bzPos,bxWidth,BLOCK_HEIGHT,bzWidth);

			}
		}
			
		// update top block details
		// create a new topBlock object
		topBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
		topBlock.name = "TopBlock:" + blockCount;
		topBlock.transform.localScale = new Vector3(xWidth, BLOCK_HEIGHT, zWidth);
		topBlock.transform.position = new Vector3 (centreX, movingBlock.transform.position.y, centreZ);
		Color mColor = movingBlock.GetComponent<Renderer>().material.color;		
		topBlock.GetComponent<Renderer> ().material.color = mColor;		



		// destroy previous movingBlock
		Destroy(movingBlock);

		// move camera
		theCamera.transform.Translate(Vector3.up * BLOCK_HEIGHT, Space.World);

		// add a new moving block
		NewMovingBlock();
		return true;
	}

	// create a new moving block
	void NewMovingBlock() {

		// create a new cube
		movingBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
		movingBlock.name = "MovingBlock";
		// scale it
		movingBlock.transform.localScale = topBlock.transform.localScale;
		// add above top block
		movingBlock.transform.position = new Vector3(topBlock.transform.position.x, topBlock.transform.position.y+BLOCK_HEIGHT, topBlock.transform.position.z);
		Color tColor = topBlock.GetComponent<Renderer>().material.color;		
		movingBlock.GetComponent<Renderer>().material.color = new Color(tColor.r-0.1f,0,0);		

		// flip dir
		movingBlockXDir = !movingBlockXDir;

	}

	// create a new broken block
	void NewBrokenBlock(float xPos, float yPos, float zPos, float xScale, float yScale, float zScale) {

		// create a new cube
		GameObject brokenBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
		brokenBlock.name = "BrokenBlock";
		// scale it
		brokenBlock.transform.localScale  = new Vector3(xScale,yScale,zScale);
		brokenBlock.transform.position = new Vector3(xPos,yPos,zPos);
		Color tColor = topBlock.GetComponent<Renderer>().material.color;		
		brokenBlock.GetComponent<Renderer>().material.color = new Color(tColor.r-0.1f,0,0);	

		// add physics
		Rigidbody rigidBody = brokenBlock.AddComponent<Rigidbody>();
		rigidBody.mass = 5;
	}

}