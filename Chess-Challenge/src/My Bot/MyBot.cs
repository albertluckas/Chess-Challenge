using ChessChallenge.API;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public class MyBot : IChessBot{
//todo: sort moves by imidiate qality
    Move bestMove;
    public Move Think(Board board, Timer timer) //Returns the move to play
    {
        
        if(timer.MillisecondsRemaining<10){
            return board.GetLegalMoves()[0];

        }
        
        int csign=board.IsWhiteToMove ? 1:-1;//Figure out what color pieces you are playing with
        if(timer.MillisecondsRemaining<100){
            checkTree(board, 2,true,-99999*csign,99999*csign,timer);
            return bestMove;
        }
        
        if(timer.MillisecondsRemaining<1000){
            checkTree(board, 3,true,-99999*csign,99999*csign,timer);
            return bestMove;
        }
        
            checkTree(board, 4,true,-99999*csign,99999*csign,timer);
            return bestMove;
    }

    public int EvalBoard(Board board){ //Evaluates the current board state
        int activesign=board.IsWhiteToMove ? 1 : -1;
        if(board.IsInCheckmate()){ //If checkmate, return infinite value
            return activesign*-999999999;
        }
        int evalScore = 0;
        int openingbonus=0;
        int endgamebonus=0;
        int totalPower=0;
        if(board.IsInCheck()){ //Favors giving checks
            evalScore += activesign*-50;

        }
        for(int i = 0; i < 64; i++){
            Piece piece = board.GetPiece(new Square(i));
            if(!piece.IsNull){
                int pieceColorModifier = piece.IsWhite ? 1 : -1;
                int piecescore=0;
                int hcenterness =(int)(3.5-Math.Abs(3.5-(i%8)));
                int vcenterness =(int)(3.5-Math.Abs(3.5-i/8));
                int vdevelop=(int)-Math.Abs(3.5-i/8-pieceColorModifier)*2;
                int progress= 4-(4-i/8)*pieceColorModifier;
                if(piece.IsPawn){ //Favors positions where pawns are closer to the other side of the board (and closer to promotion)
                    
                    piecescore = 100+progress*2;
                    openingbonus+=pieceColorModifier*progress*hcenterness;
                    endgamebonus+=pieceColorModifier*progress*progress;
                }
                if(piece.IsBishop){
                piecescore+=333;
                openingbonus+=pieceColorModifier*vdevelop;
                }
                if(piece.IsKnight){
                    piecescore+=305;
                    openingbonus+=pieceColorModifier*(hcenterness+vdevelop);
                }
                if(piece.IsRook){
                    piecescore+=563;
                    if((i==3&&board.GetKingSquare(piece.IsWhite).Index==2)||(i==5&&board.GetKingSquare(piece.IsWhite).Index==6)
                        ||(i==59&&board.GetKingSquare(piece.IsWhite).Index==58)||(i==61&&board.GetKingSquare(piece.IsWhite).Index==62)){
                        openingbonus+=pieceColorModifier*100;
                    }
                }
                if(piece.IsQueen){
                    piecescore+=950;
                    if(progress>0){
                        openingbonus-=pieceColorModifier*200;
                    }
                }
                evalScore+=piecescore*pieceColorModifier;
                totalPower+=piecescore;

                
            }

            
        }
        
        if(totalPower>=28){
            evalScore+=openingbonus;

        }
        return evalScore;
    }
    
    public int checkTree(Board board, int depth, Boolean root,int bestforcedactive, int bestforcedinactive,Timer timer){
        
        if(depth == 0){ //Base Case
            return EvalBoard(board);
        }
        Move[] moves = board.GetLegalMoves();
        if(depth>2){
            sortmoves(board, moves); //testing this line
        }
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
            if(move.IsCapture&&timer.MillisecondsRemaining>40*10000){
                depth++;
            }
            int moveEval = checkTree(board, depth-1,false,bestforcedinactive,bestforcedactive,timer); //Continue checking, decrement depth
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
    public void sortmoves(Board board, Move[] moves){
        int next=0;
        Move t;
        for (int i = 0; i < moves.Length; i++)
        {
            if(moves[i].IsCapture||moves[i].IsCastles){
                t=moves[i];
                moves[i]=moves[next];
                moves[next]=t;
                next++;
            }
        }
    }
}