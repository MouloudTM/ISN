using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

public class Snake : MonoBehaviour {

	private enum Direction {
		Left,
		Right,
		Up,
		Down
	}

	private enum State {
		Alive,
		Dead
	}

	private State state;
	private Direction gridMoveDirection;
	private Vector2Int gridPosition;

	int width = 20;
	int height = 20;
	private float gridMoveTimer;
	private float gridMoveTimerMax;
	private LevelGrid levelGrid;
	private int snakeBodySize;
	private List<SnakeMovePosition> snakeMovePositionList;
	private List<SnakeBodyPart> snakeBodyPartList;

	public void Setup(LevelGrid levelGrid) {
		this.levelGrid = levelGrid;
	}

	private void Awake() {
		gridPosition = new Vector2Int(10, 10);
		gridMoveTimerMax = .2f;
		gridMoveTimer = gridMoveTimerMax;
		gridMoveDirection = Direction.Right;

		snakeMovePositionList = new List<SnakeMovePosition>();
		snakeBodySize = 0;

		snakeBodyPartList = new List<SnakeBodyPart>();

		state = State.Alive;
	}

	private void Update() {
		switch (state) {
		case State.Alive:
			HandlerInput();
			HandleGridMovement();
			ValidateGridPosition(gridPosition);
			break;
		case State.Dead:
			break;
		}
		
		
	}
	
	private void HandlerInput() {
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			if (gridMoveDirection != Direction.Down) {
				gridMoveDirection = Direction.Up;
			}
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			if (gridMoveDirection != Direction.Up) {
				gridMoveDirection = Direction.Down;
			}
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			if (gridMoveDirection != Direction.Left) {
				gridMoveDirection = Direction.Right;
			}
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			if (gridMoveDirection != Direction.Right) {
				gridMoveDirection = Direction.Left;
			}	
		}
	}
	private void HandleGridMovement() {
		gridMoveTimer += Time.deltaTime;
		if (gridMoveTimer >= gridMoveTimerMax) {
			gridMoveTimer -= gridMoveTimerMax;

			SoundManager.PlaySound(SoundManager.Sound.SnakeMove);

			SnakeMovePosition previousSnakeMovePosition = null;
			if (snakeMovePositionList.Count > 0) {
				previousSnakeMovePosition = snakeMovePositionList[0];
			}

			SnakeMovePosition snakeMovePosition = new SnakeMovePosition(previousSnakeMovePosition, gridPosition, gridMoveDirection);
			snakeMovePositionList.Insert(0, snakeMovePosition);

			Vector2Int gridMoveDirectionVector;
			switch (gridMoveDirection) {
			default:
			case Direction.Right: 	gridMoveDirectionVector = new Vector2Int(+1, 0); break;
			case Direction.Left:	gridMoveDirectionVector = new Vector2Int(-1, 0); break;
			case Direction.Up:		gridMoveDirectionVector = new Vector2Int(0, +1); break;
			case Direction.Down:	gridMoveDirectionVector = new Vector2Int(0, -1); break;
			}

			gridPosition += gridMoveDirectionVector;

			bool snakeAteFood = levelGrid.TrySnakeEatFood(gridPosition);
			if (snakeAteFood) {
			//Le serpent a mangé de la nourriture, on grandit le corps du serpent
			snakeBodySize++;
			CreateSnakeBody();
			SoundManager.PlaySound(SoundManager.Sound.SnakeEat);
			}

			if(snakeMovePositionList.Count >= snakeBodySize + 1) {
				snakeMovePositionList.RemoveAt(snakeMovePositionList.Count - 1);
			}

			UpdateSnakeBodyParts();

			foreach (SnakeBodyPart snakeBodyPart in snakeBodyPartList) {
				Vector2Int snakeBodyPartGridPosition = snakeBodyPart.GetGridPosition();
				if (gridPosition == snakeBodyPartGridPosition) {
					// Game Over !
					state = State.Dead;
					GameHandler.SnakeDied();
					SoundManager.PlaySound(SoundManager.Sound.SnakeDie);
				}
			}
			
			transform.position = new Vector3(gridPosition.x, gridPosition.y);
			transform.eulerAngles = new Vector3(0, 0, GetAngleFromVector(gridMoveDirectionVector) -90 );

		}
	}

	private void CreateSnakeBody() {
		snakeBodyPartList.Add(new SnakeBodyPart(snakeBodyPartList.Count));
	}

	private void UpdateSnakeBodyParts() {
		for (int i = 0; i < snakeBodyPartList.Count; i++) {
			snakeBodyPartList[i].SetSnakeMovePosition(snakeMovePositionList[i]);
		}
	}
	private float GetAngleFromVector(Vector2Int dir) {
		float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		if (n < 0) n+= 360;
		return n;
	}

	public Vector2Int GetGridPosition() {
		return gridPosition;
	}
	//Donne la position de tous les éléments du serpent: tête + corps
	public List<Vector2Int> GetFullSnakeGridPositionList() {
		List<Vector2Int> gridPositionList = new List<Vector2Int>() { gridPosition };
		foreach (SnakeMovePosition snakeMovePosition in snakeMovePositionList) {
			gridPositionList.Add(snakeMovePosition.GetGridPosition());
		}
		return gridPositionList;
	}



	private class SnakeBodyPart {

		private SnakeMovePosition snakeMovePosition;
		private Transform transform;
		public SnakeBodyPart(int bodyIndex) {
		GameObject snakeBodyGameObject = new GameObject("SnakeBody", typeof(SpriteRenderer));
		snakeBodyGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.i.snakeBodySprite;
		snakeBodyGameObject.GetComponent<SpriteRenderer>().sortingOrder = -1 -bodyIndex;
		transform = snakeBodyGameObject.transform;
		}

		public void SetSnakeMovePosition(SnakeMovePosition snakeMovePosition) {
			this.snakeMovePosition = snakeMovePosition;
			
			transform.position = new Vector3(snakeMovePosition.GetGridPosition().x, snakeMovePosition.GetGridPosition().y);

			float angle;
			switch (snakeMovePosition.GetDirection()) {
			default: 
			case Direction.Up: // Va vers le Haut
				switch (snakeMovePosition.GetPreviousDirection()) {
				default: 
					angle = 0; break;
				case Direction.Right: // Allait vers la droite
					angle = 0 - 45;
					transform.position += new Vector3(-.2f, .2f);
					break;
				case Direction.Left: // Allait vers la gauche
					angle = 0 + 45;
					transform.position += new Vector3(.2f, .2f);
					break;
				}
				break;
			case Direction.Down: // Va vers le Bas
				switch (snakeMovePosition.GetPreviousDirection()) {
				default: 
					angle = 180; break;
				case Direction.Right: // Allait vers la droite
					angle = 180 - 45;
					transform.position += new Vector3(-.2f, -.2f);
					break;
				case Direction.Left: // Allait vers la gauche
					angle = 180 + 45;
					transform.position += new Vector3(.2f, -.2f);
					break;
				}
				break;
			case Direction.Left: // Va vers la Gauche
				switch (snakeMovePosition.GetPreviousDirection()) {
				default: 
					angle = -90; break;
				case Direction.Down: // Allait vers le bas
					angle = -45;
					transform.position += new Vector3(-.2f, .2f);
					break;
				case Direction.Up: // Allait vers le haut
					angle = 45;
					transform.position += new Vector3(-.2f, -.2f);
					break;
				}
				break;
			case Direction.Right: // Va vers la Droite
				switch (snakeMovePosition.GetPreviousDirection()) {
				default: 
					angle = 90; break;
				case Direction.Down: // Allait vers le bas
					angle = 45;
					transform.position += new Vector3(.2f, .2f);
					break;
				case Direction.Up: // Allait vers le haut
					angle = -45;
					transform.position += new Vector3(.2f, -.2f);
					break;
				}
				break;
			}

			transform.eulerAngles = new Vector3(0, 0, angle);
		}

		public Vector2Int GetGridPosition() {
			return snakeMovePosition.GetGridPosition();
		}
	}



	/*
	Stocke une position de mouvement du serpent
	 */
	private class SnakeMovePosition {

		private SnakeMovePosition previousSnakeMovePosition;

		private Vector2Int gridPosition;
		private Direction direction;

		public SnakeMovePosition(SnakeMovePosition previousSnakeMovePosition, Vector2Int gridPosition, Direction direction) {
			this.previousSnakeMovePosition = previousSnakeMovePosition;
			this.gridPosition = gridPosition;
			this.direction = direction;
		}

		public Vector2Int GetGridPosition() {
			return gridPosition;
		}

		public Direction GetDirection() {
            return direction;
        }

		public Direction GetPreviousDirection() {
			if (previousSnakeMovePosition == null) {
				return Direction.Right;
			}
			else {
				return previousSnakeMovePosition.direction;
			}
		}
	}

	public Vector2Int ValidateGridPosition(Vector2Int gridPosition) {
		if (gridPosition.x < 0 ) {
			state = State.Dead;
			GameHandler.SnakeDied();
			SoundManager.PlaySound(SoundManager.Sound.SnakeDie);
		}
		if (gridPosition.x > width - 1 ) {
			state = State.Dead;
			GameHandler.SnakeDied();
			SoundManager.PlaySound(SoundManager.Sound.SnakeDie);
		}
		if (gridPosition.y < 0 ) {
			state = State.Dead;
			GameHandler.SnakeDied();
			SoundManager.PlaySound(SoundManager.Sound.SnakeDie);
		}
		if (gridPosition.y > height - 1 ) {
			state = State.Dead;
			GameHandler.SnakeDied();
			SoundManager.PlaySound(SoundManager.Sound.SnakeDie);
		}
		return gridPosition;
	}
}

