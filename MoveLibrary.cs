using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;   
    
    public static class MoveLibrary {
        
        //This function takes as input a given move, either from the user or from the Search tree, and checks it for
        //legality against the rules of CHess
        public static bool check_legal(
            string[][] board,
            int start_rank,
            int start_file,
            int end_rank,
            int end_file,
            bool turn) {
        //initialize variables
            bool legal = true;
            bool valid_move = true;
            bool check = false;
            string error_string = "";

            //take inventory of the piece occupying the starting space
            string piece = board[start_rank][start_file];

        //break down the piece to determine its controller and its denomination
        //Debug.Log("piece0 = " + piece[0]);
            char piece_controller = piece[0];
            char piece_type = piece[1];

            //these cases contain the legal moves for each piece, and check the validity of the move input
            if (piece_type == 'P') {
                valid_move = pawn_move(board, start_rank, start_file, end_rank, end_file, turn);
            }
            if (piece_type == 'R') {
                valid_move = rook_move(board, start_rank, start_file, end_rank, end_file);
            }
            if (piece_type == 'N') {
                valid_move = knight_move(board, start_rank, start_file, end_rank, end_file);
            }
            if (piece_type == 'B') {
                valid_move = bishop_move(board, start_rank, start_file, end_rank, end_file);
            }
            if (piece_type == 'Q') {
                valid_move = queen_move(board, start_rank, start_file, end_rank, end_file);
            }
            if (piece_type == 'K') {
                valid_move = king_move(board, start_rank, start_file, end_rank, end_file);
            }
            //case for if the starting space does not contain a piece
            if (piece == "  ") {
                legal = false;
                //Debug.Log("No piece in that space");
            }
            //Case for if the the piece that is being moved does not belong to the moving player
            if (turn && piece_controller == 'B' || !turn && piece_controller == 'W') {
                legal = false;
                //Debug.Log("You don't control that piece");
            }
            //Case if the move proves to be illegal
            if (!valid_move) {
                legal = false;
                //Debug.Log("That piece can't move there");
            }
            //Case for trying to move a piece to a space it already occupies
            if (start_rank == end_rank && start_file == end_file) {
                legal = false;
                //Debug.Log("You must move a piece to a different space");
            }
            //This block checks if the move will result in check against the player making the move
            string save_piece1 = board[start_rank][start_file];
            string save_piece2 = board[end_rank][end_file];
            board[end_rank][end_file] = board[start_rank][start_file];
            board[start_rank][start_file] = "  ";
            pawn_promotion(board);
            if (turn) {
                check = is_check(board, 'W');
            } else {
                check = is_check(board, 'B');
            }
            board[start_rank][start_file] = save_piece1;
            board[end_rank][end_file] = save_piece2;
            //Case if the move results in check
            if (check) {
                legal = false;
                //Debug.Log("You cannot move into check");
            }
        return legal;
        }
        
        //This function contains the constraints for moving pawns
        public static bool pawn_move(
            string[][] board,
            int start_rank,
            int start_file,
            int end_rank,
            int end_file,
            bool turn) {
            //cases for white pawn movement
            if (turn) {
                //normal movement
                if (end_rank == start_rank - 1 && end_file == start_file && board[end_rank][end_file] == "  ") {
                    return true;
                } else if (start_rank == 6 && end_rank == start_rank - 2 && end_file == start_file && board[end_rank][end_file] == "  ") {
                    //first move special rule
                    return true;
                } else if (end_rank == start_rank - 1 && (end_file == start_file + 1 || end_file == start_file - 1) && board[end_rank][end_file][0] == 'B') {
                    //attack rule
                    return true;
                } else {
                    //en passant
                    //elif((start_rank == 5) and (end_rank == (start_rank - 1)) and ((end_file == (start_file + 1)) and (board[start_rank][start_file + 1] == 'BP')) or ((end_file == (start_file - 1)) and (board[start_rank][start_file - 1] == 'BP')) and (board[end_rank][end_file] == '  ')):
                    //return True            
                    return false;
                }
            } else {
                //cases for black pawn movement
                //normal movement
                if (end_rank == start_rank + 1 && end_file == start_file && board[end_rank][end_file] == "  ") {
                    return true;
                } else if (start_rank == 1 && end_rank == start_rank + 2 && end_file == start_file && board[end_rank][end_file] == "  ") {
                    //first move special rule
                    return true;
                } else if (end_rank == start_rank + 1 && (end_file == start_file + 1 || end_file == start_file - 1) && board[end_rank][end_file][0] == 'W') {
                    //attack rule
                    return true;
                } else {
                    //en passant
                    //elif((start_rank == 4) and (end_rank == (start_rank + 1)) and ((end_file == (start_file + 1)) and (board[start_rank][start_file + 1] == 'BP')) or ((end_file == (start_file - 1)) and (board[start_rank][start_file - 1] == 'BP')) and (board[end_rank][end_file] == '  ')):
                    //return True
                    return false;
                }
            }
        }
        
        //this function handles the promotion of pawns
        public static void pawn_promotion(string[][] board) {
            foreach (int i in Enumerable.Range(0, 8)) {
                if (board[0][i] == "WP") {
                    board[0][i] = "WQ";
                }
                if (board[7][i] == "BP") {
                    board[7][i] = "BQ";
                }
            }
        }
        
        public static bool rook_move(
            string[][] board,
            int start_rank,
            int start_file,
            int end_rank,
            int end_file) {
            bool path_clear = false;
            //statement ensures that an allied piece does not occupy the destination
            if (board[end_rank][end_file][0] == board[start_rank][start_file][0]) {
                path_clear = false;
            } else if (end_rank == start_rank) {
                //case for horizontal movement
                if (end_file > start_file) {
                    //loop makes sure no piece is in the way of the path
                    foreach (int i in Enumerable.Range(start_file + 1, end_file - (start_file + 1))) {
                        if (!(board[start_rank][i] == "  ")) {
                            path_clear = false;
                        }
                    }
                } else {
                    //loop makes sure no piece is in the way of the path
                    foreach (int i in Enumerable.Range(end_file + 1, start_file - (end_file + 1))) {
                        if (!(board[start_rank][i] == "  ")) {
                            path_clear = false;
                        }
                    }
                }
            } else if (end_file == start_file) {
                //case for vertical movement
                if (end_rank > start_rank) {
                    //loop makes sure no piece is in the way of the path
                    foreach (int i in Enumerable.Range(start_rank + 1, end_rank - (start_rank + 1))) {
                        if (!(board[i][start_file] == "  ")) {
                            path_clear = false;
                        }
                    }
                } else {
                    //loop makes sure no piece is in the way of the path
                    foreach (int i in Enumerable.Range(end_rank + 1, start_rank - (end_rank + 1))) {
                        if (!(board[i][start_file] == "  ")) {
                            path_clear = false;
                        }
                    }
                }
            } else {
                path_clear = false;
            }
            if (path_clear) {
                return true;
            } else {
                return false;
            }
        }
        
        public static bool knight_move(
            string[][] board,
            int start_rank,
            int start_file,
            int end_rank,
            int end_file) {
            if (!(board[end_rank][end_file][0] == board[start_rank][start_file][0]) && (end_rank == start_rank + 2 && (end_file == start_file + 1 || end_file == start_file - 1) || end_rank == start_rank - 2 && (end_file == start_file + 1 || end_file == start_file - 1) || end_file == start_file + 2 && (end_rank == start_rank + 1 || end_rank == start_rank - 1) || end_file == start_file - 2 && (end_rank == start_rank + 1 || end_rank == start_rank - 1))) {
                return true;
            } else {
                return false;
            }
        }
        
        public static bool bishop_move(
            string[][] board,
            int start_rank,
            int start_file,
            int end_rank,
            int end_file) {
            bool path_clear = false;

            //Check that destination is not occuppied by allied piece
            if (board[end_rank][end_file][0] == board[start_rank][start_file][0]) {
                path_clear = false;
            } else if (end_rank - start_rank == end_file - start_file) {
                //Case for +x +y diagonal and -x -y diagonal
                //+x +y
                if (end_rank > start_rank) {
                    //loop makes sure there is no piece in the way
                    foreach (int i in Enumerable.Range(1, end_rank - start_rank - 1)) {
                        if (!(board[start_rank + i][start_file + i] == "  ")) {
                            path_clear = false;
                        }
                    }
                } else {
                    //-x -y
                    //loop makes sure there is no piece in the way
                    foreach (int i in Enumerable.Range(1, start_rank - end_rank - 1)) {
                        if (!(board[start_rank - i][start_file - i] == "  ")) {
                            path_clear = false;
                        }
                    }
                }
            } else if (end_rank - start_rank == start_file - end_file) {
                //case for +x -y and -x +y                
                //+x -y
                if (end_rank > start_rank) {
                    //loop makes sure there is no piece in the way
                    foreach (int i in Enumerable.Range(1, end_rank - start_rank - 1)) {
                        if (!(board[start_rank + i][start_file - i] == "  ")) {
                            path_clear = false;
                        }
                    }
                } else {
                    //-x +y
                    //loop makes sure there is no piece in the way
                    foreach (int i in Enumerable.Range(1, start_rank - end_rank - 1)) {
                        if (!(board[start_rank - i][start_file + i] == "  ")) {
                            path_clear = false;
                        }
                    }
                }
            }
            else {
                path_clear = false;
            }

            if (path_clear) {
                return true;
            } else {
                return false;
            }
        }
        
        public static bool queen_move(
            string[][] board,
            int start_rank,
            int start_file,
            int end_rank,
            int end_file) {
            bool diagonal_move = bishop_move(board, start_rank, start_file, end_rank, end_file);
            bool cardinal_move = rook_move(board, start_rank, start_file, end_rank, end_file);
            if (cardinal_move || diagonal_move) {
                return true;
            } else {
                return false;
            }
        }
        
        public static bool king_move(
            string[][] board,
            int start_rank,
            int start_file,
            int end_rank,
            int end_file) {
            if (board[start_rank][start_file][0] == board[end_rank][end_file][0]) {
                return false;
            } else if (Math.Abs(end_rank - start_rank) > 1 || Math.Abs(end_file - start_file) > 1) {
                return false;
            } else {
                return true;
            }
        }
        
        public static bool is_check(string[][] board, char color) {
            bool is_target = false;
            bool turn;
            char attacker_color;
            int king_rank = 0;
            int king_file = 0;
            if (color == 'W') {
                attacker_color = 'B';
                turn = false;
            } else {
                attacker_color = 'W';
                turn = true;
            }
            foreach (int i in Enumerable.Range(0, 8)) {
                foreach (int j in Enumerable.Range(0, 8)) {
                    if (board[i][j][0] == color && board[i][j][1] == 'K') {
                        king_rank = i;
                        king_file = j;
                    }
                }
            }
            foreach (int i in Enumerable.Range(0, 8)) {
                foreach (int j in Enumerable.Range(0, 8)) {
                    if (board[i][j][0] == attacker_color) {
                        if (board[i][j][1] == 'P') {
                            is_target = pawn_move(board, i, j, king_rank, king_file, turn);
                        }
                        if (board[i][j][1] == 'R') {
                            is_target = rook_move(board, i, j, king_rank, king_file);
                        }
                        if (board[i][j][1] == 'N') {
                            is_target = knight_move(board, i, j, king_rank, king_file);
                        }
                        if (board[i][j][1] == 'B') {
                            is_target = bishop_move(board, i, j, king_rank, king_file);
                        }
                        if (board[i][j][1] == 'Q') {
                            is_target = queen_move(board, i, j, king_rank, king_file);
                        }
                        if (board[i][j][1] == 'K') {
                            is_target = king_move(board, i, j, king_rank, king_file);
                        }
                        if (is_target) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        public static bool is_stalemate(string[][] board, char color) {
            bool turn;
            bool can_move = true;
            if (color == 'W') {
                turn = true;
            } else {
                turn = false;
            }
            foreach (int i in Enumerable.Range(0, 8)) {
                foreach (int j in Enumerable.Range(0, 8)) {
                    if (board[i][j][0] == color) {
                        foreach (int k in Enumerable.Range(0, 8)) {
                            foreach (int l in Enumerable.Range(0, 8)) {
                                can_move = check_legal(board, i, j, k, l, turn);
                                if (can_move) {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
