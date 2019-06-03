using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

public class LevelGrid {

	private Vector2Int foodGridPosition;
	private GameObject foodGameObject;
	public int width;
	public int height;
	private Snake snake;
	public LevelGrid(int width, int height) {
		this.width = width;
		this.height = height;
	}

	public void Setup(Snake snake) {
		this.snake = snake;
		
		SpawnFood();
	}
	private void SpawnFood() {
		//Pour éviter que la nourriture spawn sur le serpent
		do {
		foodGridPosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
		} while (snake.GetFullSnakeGridPositionList().IndexOf(foodGridPosition) != -1);

		foodGameObject = new GameObject("Food", typeof(SpriteRenderer));
		foodGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.i.foodSprite;
		foodGameObject.transform.position = new Vector3(foodGridPosition.x, foodGridPosition.y);
	}
	//Pour détruire la nourriture quand le serpent passe dessus
	public bool TrySnakeEatFood(Vector2Int snakeGridPosition) {
		if (snakeGridPosition == foodGridPosition) {
			Object.Destroy(foodGameObject);
			SpawnFood();
			Score.AddScore();
			return true;
		}	else {
				return false;
			}
	}
}
