using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Board board;

    public GameObject gameDialogue;

    public GameObject Pokemon;

    //White Pieces
    public GameObject whiteKing;
    public GameObject whiteQueen;
    public GameObject whiteBishop;
    public GameObject whiteKnight;
    public GameObject whiteRook;
    public GameObject whitePawn;

    //Black Pieces
    public GameObject blackKing;
    public GameObject blackQueen;
    public GameObject blackBishop;
    public GameObject blackKnight;
    public GameObject blackRook;
    public GameObject blackPawn;

    //Fractured Pieces
    //White Pieces
    public GameObject FracwhiteKing;
    public GameObject FracwhiteQueen;
    public GameObject FracwhiteBishop;
    public GameObject FracwhiteKnight;
    public GameObject FracwhiteRook;
    public GameObject FracwhitePawn;

    //Black Pieces
    public GameObject FracblackKing;
    public GameObject FracblackQueen;
    public GameObject FracblackBishop;
    public GameObject FracblackKnight;
    public GameObject FracblackRook;
    public GameObject FracblackPawn;

    private GameObject[,] pieces;
    private List<GameObject> movedPawns;

    //Player Objects
    public Player white;
    public Player black;
    public Player currentPlayer;
    public Player otherPlayer;
    

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        pieces = new GameObject[8, 8];
        movedPawns = new List<GameObject>();

        white = new Player("white", true);
        black = new Player("black", false);

        currentPlayer = white;
        otherPlayer = black;

        InitialSetup();
    }

    public void showUI()
    {
        gameDialogue.GetComponent<Text>().text = "It is " + currentPlayer.name + "'s turn!";

        Debug.Log(currentPlayer.name + "s turn");
    }

    public void EndGameUI()
    {
        gameDialogue.GetComponent<Text>().text = "Checkmate! " + currentPlayer.name + " wins!";
    }

    public void CheckUI()
    {
        gameDialogue.GetComponent<Text>().text = currentPlayer.name + " is about to Check!";
    }

    public string[][] translator()
    {
        string[][] board1 = new string[][]
        {
        new string[] {"  ","  ","  ","  ","  ","  ","  ","  "},
        new string[] {"  ","  ","  ","  ","  ","  ","  ","  "},
        new string[] {"  ","  ","  ","  ","  ","  ","  ","  "},
        new string[] {"  ","  ","  ","  ","  ","  ","  ","  "},
        new string[] {"  ","  ","  ","  ","  ","  ","  ","  "},
        new string[] {"  ","  ","  ","  ","  ","  ","  ","  "},
        new string[] {"  ","  ","  ","  ","  ","  ","  ","  "},
        new string[] {"  ","  ","  ","  ","  ","  ","  ","  "},
        };

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Vector2Int gridPoint0 = Geometry.GridPoint(i,j);
                GameObject piecex = PieceAtGrid(gridPoint0);

                if (piecex == null)
                {
                    board1[j][7 - i] = "  ";
                    continue;
                }
                else if (piecex.GetComponent<Piece>().type == PieceType.King)
                {
                    if (white.pieces.Contains(piecex))
                    {
                        board1[j][7 - i] = "WK";
                    }
                    else
                    {
                        board1[j][7 - i] = "BK";
                    }
                }
                else if (piecex.GetComponent<Piece>().type == PieceType.Queen)
                {
                    if (white.pieces.Contains(piecex))
                    {
                        board1[j][7 - i] = "WQ";
                    }
                    else
                    {
                        board1[j][7 - i] = "BQ";
                    }
                }
                else if (piecex.GetComponent<Piece>().type == PieceType.Knight)
                {
                    if (white.pieces.Contains(piecex))
                    {
                        board1[j][7 - i] = "WN";
                    }
                    else
                    {
                        board1[j][7 - i] = "BN";
                    }
                }
                else if (piecex.GetComponent<Piece>().type == PieceType.Bishop)
                {
                    if (white.pieces.Contains(piecex))
                    {
                        board1[j][7 - i] = "WB";
                    }
                    else
                    {
                        board1[j][7 - i] = "BB";
                    }
                }
                else if (piecex.GetComponent<Piece>().type == PieceType.Rook)
                {
                    if (white.pieces.Contains(piecex))
                    {
                        board1[j][7 - i] = "WR";
                    }
                    else
                    {
                        board1[j][7 - i] = "BR";
                    }
                }
                else if (piecex.GetComponent<Piece>().type == PieceType.Pawn)
                {
                    if (white.pieces.Contains(piecex))
                    {
                        board1[j][7 - i] = "WP";
                    }
                    else
                    {
                        board1[j][7 - i] = "BP";
                    }
                }
            }
        }

        
        return board1;
       
    }

    public void CallAI()
    {
        string[][] board0 = translator();

        int[] a = ChessAI.computer_player(1, board0, false);

        Debug.Log("x1 = " + (7 - a[1]));
        Debug.Log("y1 = " + a[0]);
        Debug.Log("x2 = " + (7 - a[3]));
        Debug.Log("y2 = " + a[2]);

        Vector2Int startgridPoint = Geometry.GridPoint(7 - a[1], a[0]);

        GameObject piece = PieceAtGrid(startgridPoint);

        Vector2Int finishgridPoint = Geometry.GridPoint(7 - a[3], a[2]);


        GameObject pieceToCapture = PieceAtGrid(finishgridPoint);
        currentPlayer.capturedPieces.Add(pieceToCapture); //captured piece is added to list of captured pieces
        Destroy(pieceToCapture);

        pieces[startgridPoint.x, startgridPoint.y] = null;
        pieces[finishgridPoint.x, finishgridPoint.y] = piece;

        board.MovePiece(piece, finishgridPoint);

        NextPlayer();
        
    }

    private void InitialSetup() //Add all pieces to respective locations on chess board
    {
        System.Random rnd = new System.Random();
        int tempnum;
        int[] arrayNumb = new int[8];

        for (int i = 0; i < 8; i++) //White Backline
        {
            tempnum = rnd.Next(1, 5);
            if (i == 4)
            {
                AddPiece(whiteKing, white, i, 0);
                arrayNumb[i] = 6;
            }
            else if (tempnum == 1)
            {
                AddPiece(whiteQueen, white, i, 0);
                arrayNumb[i] = tempnum + 6;
            }
            else if (tempnum == 2)
            {
                AddPiece(whiteBishop, white, i, 0);
                arrayNumb[i] = tempnum + 6;
            }
            else if (tempnum == 3)
            {
                AddPiece(whiteKnight, white, i, 0);
                arrayNumb[i] = tempnum + 6;
            }
            else if (tempnum == 4)
            {
                AddPiece(whiteRook, white, i, 0);
                arrayNumb[i] = tempnum + 6;
            }
        }

        for (int i = 0; i < 8; i++) //Black Backline
        {
            if (i == 4)
            {
                AddPiece(blackKing, black, i, 7);
            }
            else if (arrayNumb[i] == 7) AddPiece(blackQueen, black, i, 7);
            else if (arrayNumb[i] == 8) AddPiece(blackBishop, black, i, 7);
            else if (arrayNumb[i] == 9) AddPiece(blackKnight, black, i, 7);
            else if (arrayNumb[i] == 10) AddPiece(blackRook, black, i, 7);
        }

        for (int i = 0; i < 8; i++) //White Frontline
        {
            tempnum = rnd.Next(1, 6);
            if (tempnum == 1)
            {
                AddPiece(whiteQueen, white, i, 1);
                arrayNumb[i] = tempnum + 6;
            }
            else if (tempnum == 2)
            {
                AddPiece(whiteBishop, white, i, 1);
                arrayNumb[i] = tempnum + 6;
            }
            else if (tempnum == 3)
            {
                AddPiece(whiteKnight, white, i, 1);
                arrayNumb[i] = tempnum + 6;
            }
            else if (tempnum == 4)
            {
                AddPiece(whiteRook, white, i, 1);
                arrayNumb[i] = tempnum + 6;
            }
            else if (tempnum == 5)
            {
                AddPiece(whitePawn, white, i, 1);
                arrayNumb[i] = tempnum + 6;
            }
        }

        for (int i = 0; i < 8; i++) //Black Backline
        {
            if (arrayNumb[i] == 7) AddPiece(blackQueen, black, i, 6);
            else if (arrayNumb[i] == 8) AddPiece(blackBishop, black, i, 6);
            else if (arrayNumb[i] == 9) AddPiece(blackKnight, black, i, 6);
            else if (arrayNumb[i] == 10) AddPiece(blackRook, black, i, 6);
            else if (arrayNumb[i] == 11) AddPiece(blackPawn, black, i, 6);
        }
    }

    public void AddPiece(GameObject prefab, Player player, int col, int row)
    {
        GameObject pieceObject = board.AddPiece(prefab, col, row);
        player.pieces.Add(pieceObject);
        pieces[col, row] = pieceObject;
    }

    public void SelectPieceAtGrid(Vector2Int gridPoint)
    {
        GameObject selectedPiece = pieces[gridPoint.x, gridPoint.y];
        if (selectedPiece)
        {
            board.SelectPiece(selectedPiece);
        }
    }

    public void SelectPiece(GameObject piece)
    {
        board.SelectPiece(piece);
    }

    public void DeselectPiece(GameObject piece)
    {
        board.DeselectPiece(piece);
    }

    //returns piece name at variable grid point
    public GameObject PieceAtGrid(Vector2Int gridPoint)
    {
        if (gridPoint.x > 7 || gridPoint.y > 7 || gridPoint.x < 0 || gridPoint.y < 0)
        {
            return null;
        }
        return pieces[gridPoint.x, gridPoint.y];
    }

    //returns grid location of piece
    public Vector2Int GridForPiece(GameObject piece)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] == piece)
                {
                    return new Vector2Int(i, j);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    public bool FriendlyPieceAt(Vector2Int gridPoint)
    {
        GameObject piece = PieceAtGrid(gridPoint);

        if (piece == null)
        {
            return false;
        }
        if (otherPlayer.pieces.Contains(piece))
        {
            return false;
        }

        return true;
    }

    public bool DoesPieceBelongToCurrentPlayer(GameObject piece)
    {
        return currentPlayer.pieces.Contains(piece);
    }

    public void Move(GameObject piece, Vector2Int gridPoint)
    {
        Piece pieceComponent = piece.GetComponent<Piece>();
        if (pieceComponent.type == PieceType.Pawn && !HasPawnMoved(piece))
        {
            movedPawns.Add(piece);
        }

        Vector2Int startGridPoint = GridForPiece(piece);
        pieces[startGridPoint.x, startGridPoint.y] = null;
        pieces[gridPoint.x, gridPoint.y] = piece;
        board.MovePiece(piece, gridPoint);
    }

    public void MoveFunction(GameObject piece, Vector2Int gridPoint)
    {
        Piece pieceComponent = piece.GetComponent<Piece>();
        if (pieceComponent.type == PieceType.Pawn && !HasPawnMoved(piece))
        {
            movedPawns.Add(piece);
        }

        Vector2Int startGridPoint = GridForPiece(piece);
        pieces[startGridPoint.x, startGridPoint.y] = null;
        pieces[gridPoint.x, gridPoint.y] = piece;
        board.MovePiece(piece, gridPoint);
    }

    public IEnumerator WaitForAttack(float delayInSecs, GameObject piece, Vector2Int gridPoint)
    {
        while (Pokemon != null)
        {
            yield return new WaitForSeconds(delayInSecs);
            MoveFunction(piece, gridPoint);
        }
    }

    //Boolean expressions to indicate whether the pawn piece has moved at least once so that it has normal movesets after 1st move
    public void PawnMoved(GameObject pawn)
    {
        movedPawns.Add(pawn);
    }

    public bool HasPawnMoved(GameObject pawn)
    {
        return movedPawns.Contains(pawn);
    }

    //Piece component from game piece and piece's current location are retrieved
    public List<Vector2Int> MovesForPiece(GameObject pieceObject)
    {
        Piece piece = pieceObject.GetComponent<Piece>();
        Vector2Int gridPoint = GridForPiece(pieceObject);
        var locations = piece.MoveLocations(gridPoint);

        // filter out offboard locations
        locations.RemoveAll(tile => tile.x < 0 || tile.x > 7
            || tile.y < 0 || tile.y > 7);

        // filter out locations with friendly piece
        locations.RemoveAll(tile => FriendlyPieceAt(tile));

        return locations;
    }

    //switching players
    public void NextPlayer()
    {
        Player tempPlayer = currentPlayer; //3 variables are used to transfer state data between players without overriting any data
        currentPlayer = otherPlayer;
        otherPlayer = tempPlayer;

        showUI();

        if (currentPlayer.name == "black")
        {
            CallAI();
        }

    }

    //capturing enemy pieces
    public void CapturePieceAt(Vector2Int gridPoint)
    {
        GameObject pieceToCapture = PieceAtGrid(gridPoint);
        Transform deadPiece = pieceToCapture.transform;
        if (pieceToCapture.GetComponent<Piece>().type == PieceType.King)
        {
            //EndGameUI();
            Debug.Log(currentPlayer.name + "wins!");

            Destroy(board.GetComponent<TileSelector>());
            Destroy(board.GetComponent<MoveSelector>());

            EndGameUI();
        }
        currentPlayer.capturedPieces.Add(pieceToCapture); //captured piece is added to list of captured pieces
        pieces[gridPoint.x, gridPoint.y] = null;
        Destroy(pieceToCapture); //the piece is removed from the scene
        deadPiece.position = deadPiece.position + new Vector3(0, 0, 0);
        //Fractured Pieces stuff
        if (currentPlayer == white)
        {
            if (pieceToCapture.GetComponent<Piece>().type == PieceType.Bishop)
            {
                var FBBishop = Instantiate(FracblackBishop, deadPiece.position, transform.rotation);
                Destroy(FBBishop, 2);
            }
            else if (pieceToCapture.GetComponent<Piece>().type == PieceType.Knight)
            {
                var FBKnight = Instantiate(FracblackKnight, deadPiece.position, transform.rotation);
                Destroy(FBKnight, 2);
            }
            else if (pieceToCapture.GetComponent<Piece>().type == PieceType.Pawn)
            {
                var FBPawn = Instantiate(FracblackPawn, deadPiece.position, transform.rotation);
                Destroy(FBPawn, 2);
            }
            else if (pieceToCapture.GetComponent<Piece>().type == PieceType.Queen)
            {
                var FBQueen = Instantiate(FracblackQueen, deadPiece.position, transform.rotation);
                Destroy(FBQueen, 2);
            }
            else if (pieceToCapture.GetComponent<Piece>().type == PieceType.Rook)
            {
                var FBRook = Instantiate(FracblackRook, deadPiece.position, transform.rotation);
                Destroy(FBRook, 2);
            }
        }
        else
        {
            if (pieceToCapture.GetComponent<Piece>().type == PieceType.Bishop)
            {
                var FWBishop = Instantiate(FracwhiteBishop, deadPiece.position, transform.rotation);
                Destroy(FWBishop, 2);
            }
            else if (pieceToCapture.GetComponent<Piece>().type == PieceType.Knight)
            {
                var FWKnight = Instantiate(FracwhiteKnight, deadPiece.position, transform.rotation);
                Destroy(FWKnight, 2);
            }
            else if (pieceToCapture.GetComponent<Piece>().type == PieceType.Pawn)
            {
                var FWPawn = Instantiate(FracwhitePawn, deadPiece.position, transform.rotation);
                Destroy(FWPawn, 2);
            }
            else if (pieceToCapture.GetComponent<Piece>().type == PieceType.Queen)
            {
                var FWQueen = Instantiate(FracwhiteQueen, deadPiece.position, transform.rotation);
                Destroy(FWQueen, 2);
            }
            else if (pieceToCapture.GetComponent<Piece>().type == PieceType.Rook)
            {
                var FWRook = Instantiate(FracwhiteRook, deadPiece.position, transform.rotation);
                Destroy(FWRook, 2);
            }
        }
    }
}
