using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
    
    public static class ChessAI {
        
        //Board Evalution, minimax, alpha-beta pruning
        //Built to run the AI player for Checkmate Scramble
        //Jadon West, 4/2/2020
        //import ipynb.fs.full.Chessboard_Scrambled
        //This function just kicks off the search tree process, and makes for an
        //interface between the game environment and the AI


        public static int[] computer_player(int depth, string[][] board, bool turn) {
            Console.WriteLine("Thinking...");
            int[] move = minimax_root_node(depth, board, turn);
            return move;
        }
        
        //this function evaluates the total value of a boardstate, so that the
        //AI can compare boardstates and find the most favorable one


        public static double evaluate_board(string[][] board) {
            double total_value = 0.0;
            //By assigning each piece a value based on its type and position
            //and then summing the value of each piece currently occupying the board
            //we can determine a mathematical value for any given board state
            foreach (int i in Enumerable.Range(0, 8)) {
                foreach (int j in Enumerable.Range(0, 8)) {
                    total_value = total_value + piece_value(board[i][j], i, j);
                }
            }
            return total_value;
        }
        
        //this function accesses the check_legal function in MoveLibrary and creates an
        //array of all the possible moves from the current given boardstate
        //This will be used in the minimax algorithm, so that we can evaluate only the 
        //legal moves of a given boardstate, rather than every combination of conceivable moves

        public static List<int> get_legal_moves(string[][] board, bool turn) {
            List<int> legal_moveset = new List<int>();

            //a, b, c and d are start_rank, start_file, end_rank and end_file, respectively
            foreach (int a in Enumerable.Range(0, 8)) {
                foreach (int b in Enumerable.Range(0, 8)) {
                    foreach (int c in Enumerable.Range(0, 8)) {
                        foreach (int d in Enumerable.Range(0, 8)) {
                            if (MoveLibrary.check_legal(board, a, b, c, d, turn)) {
                                legal_moveset.AddRange(new List<int>() {a, b, c, d});
                            }
                        }
                    }
                }
            }
            return legal_moveset;
        }
        
        //this function is used to iterate the depth of the search tree as more and more pieces
        //are progressively removed from the board. The fewer pieces remaining on the board, 
        //the deeper the tree should be able to search without incurring significant time loss.

        public static int get_depth(string[][] board) {
            int depth = 0;
            int num_pieces = 0;
            foreach (int i in Enumerable.Range(0, 8)) {
                foreach (int j in Enumerable.Range(0, 8)) {
                    if (!(board[i][j] == "  ")) {
                        num_pieces += 1;
                    }
                }
            }
            if (num_pieces <= 32 && num_pieces > 20) {
                depth = 2;
            }
            if (num_pieces <= 20 && num_pieces > 10) {
                depth = 3;
            }
            if (num_pieces <= 10 && num_pieces > 5) {
                depth = 4;
            }
            if (num_pieces <= 5) {
                depth = 5;
            }
            return depth;
        }

    //this function serves as the root node of our search tree. It first creates a 
    //copy of the game board (so the actual game is not disturbed) and then iterates
    //through the list of legal moves, checking the value of the boardstate after each
    //move. The depth of the search controls how many turns in the future it searches


    public static int[] minimax_root_node(int depth, string[][] board, bool turn) {

        string[][] imaginary_board = new string[][]
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

        int[] current_move = new int[1000];
        int[] final_move = new int[1000];

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                imaginary_board[i][j] = board[i][j];
 
            }
        }

        List<int> root_move_list = get_legal_moves(imaginary_board, turn);

        double best_move_value = Double.NegativeInfinity;
        final_move[0] = root_move_list[0];

        for (int i = 0; i < root_move_list.Count; i++)
        {
                current_move[i] = root_move_list[i];

                //move the piece
                string placeholder1 = imaginary_board[current_move[0]][current_move[1]];
                string placeholder2 = imaginary_board[current_move[2]][current_move[3]];
                imaginary_board[current_move[2]][current_move[3]] = imaginary_board[current_move[0]][current_move[1]];
                imaginary_board[current_move[0]][current_move[1]] = "  ";
                MoveLibrary.pawn_promotion(imaginary_board);
                double move_value = Math.Max(best_move_value, minimax_leaf_node(depth - 1, imaginary_board, Double.NegativeInfinity, Double.PositiveInfinity, false));
           // Debug.Log("move value = " + move_value);


                //move the piece back
                imaginary_board[current_move[0]][current_move[1]] = placeholder1;
                imaginary_board[current_move[2]][current_move[3]] = placeholder2;
                if (move_value > best_move_value) {
                    final_move = current_move;
                    best_move_value = move_value;
                }
            }


        return final_move;
        
        }
        
        //This function serves as the leaf nodes for the search tree. It works by iterating through
        //potential descendant boardstates and evaluating them at a specified depth, using the alpha
        //and beta factors to ignore possibilities that do not measure up to current expectations

        public static double minimax_leaf_node(
            int depth,
            string[][] board,
            double alpha,
            double beta,
            bool turn) {
            string placeholder2;
            string placeholder1;
            double best_move_value;

            int[] current_move = new int[4];
            if (depth == 0) {
                return evaluate_board(board);
            }
            if (MoveLibrary.is_check(board, 'W') && MoveLibrary.is_stalemate(board, 'W')) {
                //print("found Checkmate")
                return Double.PositiveInfinity;
            } else if (MoveLibrary.is_check(board, 'B') && MoveLibrary.is_stalemate(board, 'B')) {
                return Double.NegativeInfinity;
            } else if (MoveLibrary.is_stalemate(board, 'W') || MoveLibrary.is_stalemate(board, 'B')) {
                //print("found stalemate")
                return 0.0;
            }
            //print(board)
            List<int> leaf_move_list = get_legal_moves(board, turn);
            //maximizing case
            if (turn == false) {
                best_move_value = Double.NegativeInfinity;
                foreach (int i in Enumerable.Range(0, leaf_move_list.Count)) {
                    current_move[i] = leaf_move_list[i];
                    //move the piece
                    placeholder1 = board[current_move[0]][current_move[1]];
                    placeholder2 = board[current_move[2]][current_move[3]];
                    board[current_move[2]][current_move[3]] = board[current_move[0]][current_move[1]];
                    board[current_move[0]][current_move[1]] = "  ";
                    MoveLibrary.pawn_promotion(board);
                    best_move_value = Math.Max(best_move_value, minimax_leaf_node(depth - 1, board, alpha, beta, !turn));
                    //move the piece back
                    board[current_move[0]][current_move[1]] = placeholder1;
                    board[current_move[2]][current_move[3]] = placeholder2;
                    alpha = Math.Max(alpha, best_move_value);
                    if (beta <= alpha) {
                        return best_move_value;
                    }
                }
                return best_move_value;
            } else {
                //minimizing case
                best_move_value = Double.PositiveInfinity;
                foreach (int i in Enumerable.Range(0, leaf_move_list.Count)) {
                    current_move[i] = leaf_move_list[i];
                    //move the piece
                    placeholder1 = board[current_move[0]][current_move[1]];
                    placeholder2 = board[current_move[2]][current_move[3]];
                    board[current_move[2]][current_move[3]] = board[current_move[0]][current_move[1]];
                    board[current_move[0]][current_move[1]] = "  ";
                    MoveLibrary.pawn_promotion(board);
                    best_move_value = Math.Min(best_move_value, minimax_leaf_node(depth - 1, board, alpha, beta, !turn));
                    //move the piece back
                    board[current_move[0]][current_move[1]] = placeholder1;
                    board[current_move[2]][current_move[3]] = placeholder2;
                    beta = Math.Min(beta, best_move_value);
                    if (beta <= alpha) {
                        return best_move_value;
                    }
                }
                return best_move_value;
            }
        }
        
        //This function obtains a value for each piece in a specific location
        //To be used in the evaluation of the boardstate

        public static double piece_value(string piece, int rank, int file) {
        //first, we generate the value functions of each piece based on position
        //This set of tables is preliminary and can be changed later
        List<List<double>> black_pawn_table = new List<List<double>> {
                new List<double> {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0
                },
                new List<double> {
                    5.0,
                    5.0,
                    5.0,
                    5.0,
                    5.0,
                    5.0,
                    5.0,
                    5.0
                },
                new List<double> {
                    1.0,
                    1.0,
                    2.0,
                    3.0,
                    3.0,
                    2.0,
                    1.0,
                    1.0
                },
                new List<double> {
                    0.5,
                    0.5,
                    1.0,
                    2.5,
                    2.5,
                    1.0,
                    0.5,
                    0.5
                },
                new List<double> {
                    0.0,
                    0.0,
                    0.0,
                    2.0,
                    2.0,
                    0.0,
                    0.0,
                    0.0
                },
                new List<double> {
                    0.5,
                    -0.5,
                    -1.0,
                    0.0,
                    0.0,
                    -1.0,
                    -0.5,
                    0.5
                },
                new List<double> {
                    0.5,
                    1.0,
                    1.0,
                    -2.0,
                    -2.0,
                    1.0,
                    1.0,
                    0.5
                },
                new List<double> {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0
                }
            };
        //Each value table for white pieces should be the equivalent of the black table
        //for the corresponding piece, flipped vertically and negated
        List<List<double>> white_pawn_table = new List<List<double>> {
                new List<double> {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0
                },
                new List<double> {
                    -0.5,
                    -1.0,
                    -1.0,
                    2.0,
                    2.0,
                    -1.0,
                    -1.0,
                    -0.5
                },
                new List<double> {
                    -0.5,
                    0.5,
                    1.0,
                    0.0,
                    0.0,
                    1.0,
                    0.5,
                    -0.5
                },
                new List<double> {
                    0.0,
                    0.0,
                    0.0,
                    -2.0,
                    -2.0,
                    0.0,
                    0.0,
                    0.0
                },
                new List<double> {
                    -0.5,
                    -0.5,
                    -1.0,
                    -2.5,
                    -2.5,
                    -1.0,
                    -0.5,
                    -0.5
                },
                new List<double> {
                    -1.0,
                    -1.0,
                    -2.0,
                    -3.0,
                    -3.0,
                    -2.0,
                    -1.0,
                    -1.0
                },
                new List<double> {
                    -5.0,
                    -5.0,
                    -5.0,
                    -5.0,
                    -5.0,
                    -5.0,
                    -5.0,
                    -5.0
                },
                new List<double> {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0
                }
            };
        List<List<double>> black_knight_table = new List<List<double>> {
                new List<double> {
                    -5.0,
                    -4.0,
                    -3.0,
                    -3.0,
                    -3.0,
                    -3.0,
                    -4.0,
                    -4.0
                },
                new List<double> {
                    -4.0,
                    -2.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -2.0,
                    -4.0
                },
                new List<double> {
                    -3.0,
                    0.0,
                    1.0,
                    1.5,
                    1.5,
                    1.0,
                    0.0,
                    -3.0
                },
                new List<double> {
                    -3.0,
                    0.5,
                    1.5,
                    2.0,
                    2.0,
                    1.5,
                    0.5,
                    -3.0
                },
                new List<double> {
                    -3.0,
                    0.0,
                    1.5,
                    2.0,
                    2.0,
                    1.5,
                    0.0,
                    -3.0
                },
                new List<double> {
                    -3.0,
                    0.5,
                    1.0,
                    1.5,
                    1.5,
                    1.0,
                    0.5,
                    -3.0
                },
                new List<double> {
                    -4.0,
                    -2.0,
                    0.0,
                    0.5,
                    0.5,
                    0.0,
                    -2.0,
                    -4.0
                },
                new List<double> {
                    -5.0,
                    -4.0,
                    -3.0,
                    -3.0,
                    -3.0,
                    -3.0,
                    -4.0,
                    -5.0
                }
            };
        List<List<double>> white_knight_table = new List<List<double>> {
                new List<double> {
                    5.0,
                    4.0,
                    3.0,
                    3.0,
                    3.0,
                    3.0,
                    4.0,
                    5.0
                },
                new List<double> {
                    4.0,
                    2.0,
                    0.0,
                    -0.5,
                    -0.5,
                    0.0,
                    2.0,
                    4.0
                },
                new List<double> {
                    3.0,
                    -0.5,
                    -1.0,
                    -1.5,
                    -1.5,
                    -1.0,
                    -0.5,
                    3.0
                },
                new List<double> {
                    3.0,
                    0.0,
                    -1.5,
                    -2.0,
                    -2.0,
                    -1.5,
                    0.0,
                    3.0
                },
                new List<double> {
                    3.0,
                    -0.5,
                    -1.5,
                    -2.0,
                    -2.0,
                    -1.5,
                    -0.5,
                    3.0
                },
                new List<double> {
                    3.0,
                    0.0,
                    -1.0,
                    -1.5,
                    -1.5,
                    -1.0,
                    0.0,
                    3.0
                },
                new List<double> {
                    4.0,
                    2.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    2.0,
                    4.0
                },
                new List<double> {
                    5.0,
                    4.0,
                    3.0,
                    3.0,
                    3.0,
                    3.0,
                    4.0,
                    4.0
                }
            };
        List<List<double>> black_bishop_table = new List<List<double>> {
                new List<double> {
                    -2.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -2.0
                },
                new List<double> {
                    -1.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -1.0
                },
                new List<double> {
                    -1.0,
                    0.0,
                    0.5,
                    1.0,
                    1.0,
                    0.5,
                    0.0,
                    -1.0
                },
                new List<double> {
                    -1.0,
                    0.5,
                    0.5,
                    1.0,
                    1.0,
                    0.5,
                    0.5,
                    -1.0
                },
                new List<double> {
                    -1.0,
                    0.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    0.0,
                    -1.0
                },
                new List<double> {
                    -1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    -1.0
                },
                new List<double> {
                    -1.0,
                    0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.5,
                    -1.0
                },
                new List<double> {
                    -2.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -2.0
                }
            };
        List<List<double>> white_bishop_table = new List<List<double>> {
                new List<double> {
                    2.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    2.0
                },
                new List<double> {
                    1.0,
                    -0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -0.5,
                    1.0
                },
                new List<double> {
                    1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    1.0
                },
                new List<double> {
                    1.0,
                    0.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    0.0,
                    1.0
                },
                new List<double> {
                    1.0,
                    -0.5,
                    -0.5,
                    -1.0,
                    -1.0,
                    -0.5,
                    -0.5,
                    1.0
                },
                new List<double> {
                    1.0,
                    0.0,
                    -0.5,
                    -1.0,
                    -1.0,
                    -0.5,
                    0.0,
                    1.0
                },
                new List<double> {
                    1.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    1.0
                },
                new List<double> {
                    2.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    2.0
                }
            };
        List<List<double>> black_rook_table = new List<List<double>> {
                new List<double> {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0
                },
                new List<double> {
                    0.5,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    1.0,
                    0.5
                },
                new List<double> {
                    -0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -0.5
                },
                new List<double> {
                    -0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -0.5
                },
                new List<double> {
                    -0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -0.5
                },
                new List<double> {
                    -0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -0.5
                },
                new List<double> {
                    -0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -0.5
                },
                new List<double> {
                    0.0,
                    0.0,
                    0.0,
                    0.5,
                    0.5,
                    0.0,
                    0.0,
                    0.0
                }
            };
        List<List<double>> white_rook_table = new List<List<double>> {
                new List<double> {
                    0.0,
                    0.0,
                    0.0,
                    -0.5,
                    -0.5,
                    0.0,
                    0.0,
                    0.0
                },
                new List<double> {
                    0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.5
                },
                new List<double> {
                    0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.5
                },
                new List<double> {
                    0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.5
                },
                new List<double> {
                    0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.5
                },
                new List<double> {
                    0.5,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.5
                },
                new List<double> {
                    -0.5,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -1.0,
                    -0.5
                },
                new List<double> {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0
                }
            };
        List<List<double>> black_queen_table = new List<List<double>> {
                new List<double> {
                    -2.0,
                    -1.0,
                    -1.0,
                    -0.5,
                    -0.5,
                    -1.0,
                    -1.0,
                    -2.0
                },
                new List<double> {
                    -1.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -1.0
                },
                new List<double> {
                    -1.0,
                    0.0,
                    0.5,
                    0.5,
                    0.5,
                    0.5,
                    0.0,
                    -1.0
                },
                new List<double> {
                    -0.5,
                    0.0,
                    0.5,
                    0.5,
                    0.5,
                    0.5,
                    0.0,
                    -0.5
                },
                new List<double> {
                    0.0,
                    0.0,
                    0.5,
                    0.5,
                    0.5,
                    0.5,
                    0.0,
                    0.0
                },
                new List<double> {
                    -1.0,
                    0.0,
                    0.5,
                    0.5,
                    0.5,
                    0.5,
                    0.0,
                    -1.0
                },
                new List<double> {
                    -1.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -1.0
                },
                new List<double> {
                    -2.0,
                    -1.0,
                    -0.5,
                    -0.5,
                    -0.5,
                    -1.0,
                    -1.0,
                    -2.0
                }
            };
        List<List<double>> white_queen_table = new List<List<double>> {
                new List<double> {
                    2.0,
                    1.0,
                    0.5,
                    0.5,
                    0.5,
                    1.0,
                    1.0,
                    2.0
                },
                new List<double> {
                    1.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    1.0
                },
                new List<double> {
                    1.0,
                    0.0,
                    -0.5,
                    -0.5,
                    -0.5,
                    -0.5,
                    0.0,
                    1.0
                },
                new List<double> {
                    0.0,
                    0.0,
                    -0.5,
                    -0.5,
                    -0.5,
                    -0.5,
                    0.0,
                    0.0
                },
                new List<double> {
                    0.5,
                    0.0,
                    -0.5,
                    -0.5,
                    -0.5,
                    -0.5,
                    0.0,
                    0.5
                },
                new List<double> {
                    1.0,
                    0.0,
                    -0.5,
                    -0.5,
                    -0.5,
                    -0.5,
                    0.0,
                    1.0
                },
                new List<double> {
                    1.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    1.0
                },
                new List<double> {
                    2.0,
                    1.0,
                    1.0,
                    0.5,
                    0.5,
                    1.0,
                    1.0,
                    2.0
                }
            };
        List<List<double>> black_king_table = new List<List<double>> {
                new List<double> {
                    -3.0,
                    -4.0,
                    -4.0,
                    -5.0,
                    -5.0,
                    -4.0,
                    -4.0,
                    -3.0
                },
                new List<double> {
                    -3.0,
                    -4.0,
                    -4.0,
                    -5.0,
                    -5.0,
                    -4.0,
                    -4.0,
                    -3.0
                },
                new List<double> {
                    -3.0,
                    -4.0,
                    -4.0,
                    -5.0,
                    -5.0,
                    -4.0,
                    -4.0,
                    -3.0
                },
                new List<double> {
                    -3.0,
                    -4.0,
                    -4.0,
                    -5.0,
                    -5.0,
                    -4.0,
                    -4.0,
                    -3.0
                },
                new List<double> {
                    -2.0,
                    -3.0,
                    -3.0,
                    -4.0,
                    -4.0,
                    -3.0,
                    -3.0,
                    -2.0
                },
                new List<double> {
                    -1.0,
                    -2.0,
                    -2.0,
                    -2.0,
                    -2.0,
                    -2.0,
                    -2.0,
                    -1.0
                },
                new List<double> {
                    2.0,
                    2.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    2.0,
                    2.0
                },
                new List<double> {
                    2.0,
                    3.0,
                    1.0,
                    0.0,
                    0.0,
                    1.0,
                    3.0,
                    2.0
                }
            };
        List<List<double>> white_king_table = new List<List<double>> {
                new List<double> {
                    -2.0,
                    -3.0,
                    -1.0,
                    0.0,
                    0.0,
                    -1.0,
                    -3.0,
                    -2.0
                },
                new List<double> {
                    -2.0,
                    -2.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    -2.0,
                    -2.0
                },
                new List<double> {
                    1.0,
                    2.0,
                    2.0,
                    2.0,
                    2.0,
                    2.0,
                    2.0,
                    1.0
                },
                new List<double> {
                    2.0,
                    3.0,
                    3.0,
                    4.0,
                    4.0,
                    3.0,
                    3.0,
                    2.0
                },
                new List<double> {
                    3.0,
                    4.0,
                    4.0,
                    5.0,
                    5.0,
                    4.0,
                    4.0,
                    3.0
                },
                new List<double> {
                    3.0,
                    4.0,
                    4.0,
                    5.0,
                    5.0,
                    4.0,
                    4.0,
                    3.0
                },
                new List<double> {
                    3.0,
                    4.0,
                    4.0,
                    5.0,
                    5.0,
                    4.0,
                    4.0,
                    3.0
                },
                new List<double> {
                    3.0,
                    4.0,
                    4.0,
                    5.0,
                    5.0,
                    4.0,
                    4.0,
                    3.0
                }
            };
            //Here we add the values for each piece based on position to a static value
            //returning that value to acquire the proper evaluation for any given piece
            //these static values are based on widely accepted figures among chess players and 
            //programmers alike, but can be changed if a more favorable evaluation is found

            if (piece == "BP") {
                return 10.0 + black_pawn_table[rank][file];
            }
            if (piece == "WP") {
                return -10.0 + white_pawn_table[rank][file];
            }
            if (piece == "BN") {
                return 30.0 + black_knight_table[rank][file];
            }
            if (piece == "WN") {
                return -30.0 + white_knight_table[rank][file];
            }
            if (piece == "BB") {
                return 30.0 + black_bishop_table[rank][file];
            }
            if (piece == "WB") {
                return -30.0 + white_bishop_table[rank][file];
            }
            if (piece == "BR") {
                return 50.0 + black_rook_table[rank][file];
            }
            if (piece == "WR") {
                return -50.0 + white_rook_table[rank][file];
            }
            if (piece == "BQ") {
                return 90.0 + black_queen_table[rank][file];
            }
            if (piece == "WQ") {
                return -90.0 + white_queen_table[rank][file];
            }
            if (piece == "BK") {
                return 900.0 + black_king_table[rank][file];
            }
            if (piece == "WK") {
                return -900.0 + white_king_table[rank][file];
            }
            if (piece == "  ") {
                return 0.0;
            }
            else
            {
            return 0.0;
            }
        }
    }