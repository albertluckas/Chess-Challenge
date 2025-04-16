﻿using ChessChallenge.API;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public class MyBot : IChessBot{

    Boolean playingAsWhite;
    Move bestMove;
    public Move Think(Board board, Timer timer) //Returns the move to play
    {
        
        int csign=board.IsWhiteToMove ? 1:-1;//Figure out what color pieces you are playing with
        checkTree(board, 4,true,-9999999*csign,9999999*csign);
        return bestMove;
    }

    public int EvalBoard(Board board){ //Evaluates the current board state
        if(board.IsInCheckmate()){ //If checkmate, return infinite value
            return board.IsWhiteToMove ? -9999999 : 9999999;
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
                int hcenterness =(int)(3.5-Math.Abs(3.5-(i%8)));
                int vcenterness =(int)(3.5-Math.Abs(3.5-i/8));
                int vdevelop=(int)-Math.Abs(3.5-i/8)*2;
                int progress= (int)(3.5-3.5*pieceColorModifier+pieceColorModifier*i/8);
                if(piece.IsPawn){ //Favors positions where pawns are closer to the other side of the board (and closer to promotion)
                    
                    piecescore = 100+progress+progress*hcenterness/2;
                    
                }
                if(piece.IsBishop){
                piecescore+=302+vdevelop;
                }
                if(piece.IsKnight){
                    piecescore+=300+hcenterness+vdevelop;
                }
                if(piece.IsRook){
                    piecescore+=500;
                }
                if(piece.IsQueen){
                    piecescore+=900;
                }
                if(piece.IsKing&&(i%8==2||i%8==6)){
                    piecescore+=20;
                }
                evalScore+=piecescore*pieceColorModifier;

                
            }

            
        }
        return evalScore;
    }


    public int checkTree(Board board, int depth, Boolean root,int bestforcedactive, int bestforcedinactive){
        
        if(depth == 0){ //Base Case
            return EvalBoard(board);
        }
        Move[] moves = board.GetLegalMoves();
        int csign=board.IsWhiteToMove ? 1:-1;
        if(root){
            bestMove=moves[0];
        }
        int bestEval =-999999*csign;
        if(board.IsInCheckmate()){
            return bestEval;
        }
        foreach(Move move in moves){

            board.MakeMove(move);
            int moveEval = checkTree(board, depth-1,false,bestforcedinactive,bestforcedactive); //Continue checking, decrement depth
            board.UndoMove(move);

            if(csign*moveEval > csign*bestEval){ 
                bestEval = moveEval;
                if(root){bestMove = move;}

                if(csign*bestforcedactive > csign*bestforcedinactive){ 
                    return 99999*csign;
                }
                if(csign*bestEval > csign*bestforcedactive){ 
                    bestforcedactive=bestEval;
                }  
            }
                      
        }
        
        return bestEval;
    }
}