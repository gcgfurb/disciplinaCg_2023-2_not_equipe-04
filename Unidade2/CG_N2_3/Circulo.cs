#define CG_Debug

using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;
using System;

namespace gcgcg
{
  internal class Circulo : Objeto
  {
    public Circulo(Objeto paiRef, ref char _rotulo, Ponto4D ptoCentro, double raio, int numPontos, bool drawLines = false) : base(paiRef, ref _rotulo)
    {
      PrimitivaTipo = PrimitiveType.Points;
      PrimitivaTamanho = (float)5;
      double anguloPorPonto = 360/numPontos;

      for (int i = 0; i < numPontos; i++)
      {
        //double ptoX = (raio * Math.Cos(anguloPorPonto*i));
        //double ptoY = (raio * Math.Sin(anguloPorPonto*i));
        double ptoX = (Math.Cos(Math.PI * (anguloPorPonto * i) / 180.0) * raio);
        double ptoY = (Math.Sin(Math.PI * (anguloPorPonto * i) / 180.0) * raio);
        base.PontosAdicionar(new Ponto4D(ptoX, ptoY));
      }
      // Sentido horário
      //base.PontosAdicionar(ptoInfEsq);
      //base.PontosAdicionar(new Ponto4D(ptoSupDir.X, ptoInfEsq.Y));
      //base.PontosAdicionar(ptoSupDir);
      //base.PontosAdicionar(new Ponto4D(ptoInfEsq.X, ptoSupDir.Y));
      Atualizar();
    }

    private void Atualizar()
    {

      base.ObjetoAtualizar();
    }

#if CG_Debug
    public override string ToString()
    {
      string retorno;
      retorno = "__ Objeto Circulo _ Tipo: " + PrimitivaTipo + " _ Tamanho: " + PrimitivaTamanho + "\n";
      retorno += base.ImprimeToString();
      return (retorno);
    }
#endif

  }
}
