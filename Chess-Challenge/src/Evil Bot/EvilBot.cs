using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {


    Boolean playingAsWhite;
    public Move Think(Board board, Timer timer) //Returns the move to play
    {
        playingAsWhite = board.IsWhiteToMove; //Figure out what color pieces you are playing with
        return chooseMove(board, 2);
    }

    public int EvalBoard(Board board){ //Evaluates the current board state
        int evalScore = 0;
        if(board.IsInCheckmate()){ //If checkmate, add infinite value
                return(board.IsWhiteToMove ? -99999 : 99999);
            }
        for(int i = 0; i < 64; i++){
            Piece piece = board.GetPiece(new Square(i));
            int pieceColorModifier = piece.IsWhite ? 1 : -1;
            if(!piece.IsNull){
                if(piece.IsPawn){ //Favors positions where pawns are closer to the other side of the board (and closer to promotion)
                    if(pieceColorModifier == 1){
                        evalScore += (i/8);
                    }
                    else{
                        evalScore += (7 - (i/8))*pieceColorModifier;
                    }
                }
                else{ //Favors positions where pieces (except for the king) have been moved from their starting position
                    if(!piece.IsKing){
                        if(pieceColorModifier == 1 && i > 7){
                            evalScore += 2;
                        }
                        if(pieceColorModifier == -1 && i < 56){
                            evalScore -= 2;
                        }
                        
                    }
                }
                
            }
            if(piece.IsPawn){
                evalScore += 100 * pieceColorModifier;
                evalScore += (3 - Math.Min(Math.Abs((i%8) - 4), Math.Abs((i%8) - 3))) * pieceColorModifier; //Favors pawns in the center of the board
            }
            if(piece.IsBishop){
                evalScore += 302 * pieceColorModifier;
            }
            if(piece.IsKnight){
                evalScore += 300 * pieceColorModifier;
            }
            if(piece.IsRook){
                evalScore += 500 * pieceColorModifier;
            }
            if(piece.IsQueen){
                evalScore += 900 * pieceColorModifier;
            }
            if(piece.IsKing){
                evalScore += 10000 * pieceColorModifier;
            }
            
            if(board.IsInCheck()){ //Favors giving checks
                evalScore += board.IsWhiteToMove ? -10 : 10;
            }
        }
        return evalScore;
    }

    public Move chooseMove(Board board, int maxDepth){
        Random rng = new();
        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];

        int bestEval = playingAsWhite ? -99999 : 99999; 

        foreach(Move move in moves){

            board.MakeMove(move); //Tries the first move, then checks further outcomes
            int moveEval = checkTree(board, maxDepth); //Activate recursion to check deeper
            board.UndoMove(move); //Undoes move to reset for next move it checks

            if(playingAsWhite ? (moveEval > bestEval) : (moveEval < bestEval)){ //For the person moving, find the best move they can make (most + for white, most - for black)
                bestEval = moveEval;
                bestMove = move;
            }
            else if(moveEval == bestEval){ //If two positions are evaluated the same, choose randomly between them
                if(rng.Next(2) == 0){
                    bestMove = move;
                }
            }

        }
        return bestMove;
    }

    public int checkTree(Board board, int depth){
        if(depth == 0){ //Base Case
            return EvalBoard(board);
        }

        Move[] moves = board.GetLegalMoves();

        int bestEval = board.IsWhiteToMove ? -99999 : 99999;

        foreach(Move move in moves){

            board.MakeMove(move);
            int moveEval = checkTree(board, depth-1); //Continue checking, decrement depth
            board.UndoMove(move);

            if(board.IsWhiteToMove ? (moveEval > bestEval) : (moveEval < bestEval)){ 
                bestEval = moveEval;
            }
            
        }
        return bestEval;
    }
}}