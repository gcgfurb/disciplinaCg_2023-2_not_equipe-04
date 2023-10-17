#define CG_Debug

using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;
using System;

namespace gcgcg
{
  internal class SrPalito : Objeto
  {
    public double Raio { get; set; }
    public int Angulo { get; set; }

    public SrPalito(Objeto paiRef, ref char _rotulo) : base(paiRef, ref _rotulo)
    {
        Raio = 0.5;
        Angulo = 45;
        PrimitivaTipo = PrimitiveType.Lines;
        PrimitivaTamanho = 1;

        base.PontosAdicionar(new Ponto4D(0.0,0.0));
        base.PontosAdicionar(GetCabeca());
        Atualizar();
    }

    public void MoverHorizontal(double xMovement){
        base.pontosLista[0].X += xMovement;
        base.pontosLista[1].X += xMovement;
        Atualizar();
    }

    public void MudarTamanho(double lenghtToChange){
        Raio += lenghtToChange;
        if(Raio < 0.1)
          Raio = 0.1;
        base.pontosLista[1] = GetCabeca();
        Atualizar();
    }

    public void Girar(int graus){
        Angulo += graus;
        while(Angulo < 0){
          Angulo += 360;
        }
        while(Angulo >= 360){
          Angulo -= 360;
        }
        base.pontosLista[1] = GetCabeca();
        Atualizar();
    }

    private Ponto4D GetCabeca(){
        Ponto4D pontoBase = base.pontosLista[0];
        double ptoX = (Math.Cos(Math.PI * Angulo / 180.0) * Raio) + pontoBase.X;
        double ptoY = (Math.Sin(Math.PI * Angulo / 180.0) * Raio) + pontoBase.Y;
        return new Ponto4D(ptoX, ptoY);
    }

    private void Atualizar()
    {
      base.ObjetoAtualizar();
    }

#if CG_Debug
    public override string ToString()
    {
      string retorno;
      retorno = "__ Objeto SrPalito _ Tipo: " + PrimitivaTipo + " _ Tamanho: " + PrimitivaTamanho + "\n";
      retorno += base.ImprimeToString();
      return (retorno);
    }
#endif
  }
}
