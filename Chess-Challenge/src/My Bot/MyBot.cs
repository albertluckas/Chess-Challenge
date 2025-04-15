using ChessChallenge.API;
using System;
using System.Security.Cryptography.X509Certificates;

public class MyBot : IChessBot
{

    Boolean playingAsWhite;
    Move bestMove;
    public Move Think(Board board, Timer timer) //Returns the move to play
    {
        playingAsWhite = board.IsWhiteToMove; //Figure out what color pieces you are playing with
        return chooseMove(board, 20);
    }

    public int EvalBoard(Board board){ //Evaluates the current board state
        if(board.IsInCheckmate()){ //If checkmate, return infinite value
            return(board.IsWhiteToMove ? -9999999 : 9999999);
        }
        int evalScore = 0;
        if(board.IsInCheck()){ //Favors giving checks
            evalScore += board.IsWhiteToMove ? -50 : 50;
        }
        for(int i = 0; i < 64; i++){
            Piece piece = board.GetPiece(new Square(i));
            if(!piece.IsNull){
                int pieceColorModifier = piece.IsWhite ? 1 : -1;
                int piecescore=0;
                int hcenterness =(4-Math.Abs(4-(i%8)));
                int progress= (int)(3.5-3.5*pieceColorModifier+pieceColorModifier*i/8);
                if(piece.IsPawn){ //Favors positions where pawns are closer to the other side of the board (and closer to promotion)
                    
                    piecescore = 100+progress+progress*hcenterness/2;
                    
                }
                else{ //Favors positions where pieces (except for the king) have been moved from their starting position
                    if(!piece.IsKing&&!piece.IsRook){
                        if(progress>0){
                            piecescore+=10;
                        }
                        
                    }
                }
                if(piece.IsBishop){
                piecescore+=302;
            }
            if(piece.IsKnight){
                piecescore+=300;
            }
            if(piece.IsRook){
                piecescore+=500;
            }
            if(piece.IsQueen){
                piecescore+=900;
            }
            evalScore+=piecescore*pieceColorModifier;

                
            }

            
        }
        return evalScore;
    }

    public Move chooseMove(Board board, int maxDepth){
        Random rng = new();
        Move[] moves = board.GetLegalMoves();
        bestMove = moves[0];

        int bestEval = playingAsWhite ? -99999 : 99999; 

        checkTree(board,3,true);
        return bestMove;
    }

    public int checkTree(Board board, int depth, Boolean root){
        if(depth == 0){ //Base Case
            return EvalBoard(board);
        }

        Move[] moves = board.GetLegalMoves();

        int bestEval = board.IsWhiteToMove ? -99999 : 99999;
        foreach(Move move in moves){

            board.MakeMove(move);
            int moveEval = checkTree(board, depth-1,false); //Continue checking, decrement depth
            board.UndoMove(move);

            if(board.IsWhiteToMove ? (moveEval > bestEval) : (moveEval < bestEval)){ 
                bestEval = moveEval;
                if(root){bestMove = move;}
            }
            
        }
        
        return bestEval;
    }
}