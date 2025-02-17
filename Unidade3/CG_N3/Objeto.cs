#define CG_OpenGL
#define CG_Debug
// #define CG_DirectX

using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace gcgcg
{
  internal class Objeto  // TODO: deveria ser uma class abstract ..??
  {
    // Objeto
    private readonly char rotulo;
    protected Objeto paiRef;
    private List<Objeto> objetosLista = new List<Objeto>();
    private PrimitiveType primitivaTipo = PrimitiveType.LineLoop;
    public PrimitiveType PrimitivaTipo { get => primitivaTipo; set => primitivaTipo = value; }
    private float primitivaTamanho = 1;
    public float PrimitivaTamanho { get => primitivaTamanho; set => primitivaTamanho = value; }
    private Shader _shaderObjeto = new Shader("Shaders/shader.vert", "Shaders/shaderBranca.frag");
    public Shader shaderObjeto { set => _shaderObjeto = value; }

    // Vértices do objeto TODO: o objeto mundo deveria ter estes atributos abaixo?
    protected List<Ponto4D> pontosLista = new List<Ponto4D>();
    private int _vertexBufferObject;
    private int _vertexArrayObject;

    // BBox do objeto
    private BBox bBox = new BBox();
    private Objeto ObjetoBbox;
    private Objeto ObjetoBboxCentro;

    public BBox Bbox()  // TODO: readonly
    {
      return bBox;
    }

    // Transformações do objeto
    private Transformacao4D matriz = new Transformacao4D();

    public Objeto(Objeto paiRef, ref char _rotulo, Objeto objetoFilho = null)
    {
      this.paiRef = paiRef;
      rotulo = _rotulo = Utilitario.CharProximo(_rotulo);
      if (paiRef != null)
      {
        ObjetoAdicionar(objetoFilho);
      }
    }

    private void ObjetoAdicionar(Objeto objetoFilho)
    {
      if (objetoFilho == null)
      {
        paiRef.objetosLista.Add(this);
      }
      else
      {
        paiRef.FilhoAdicionar(objetoFilho);
      }
    }

    public void ObjetoAtualizar()
    {
      float[] vertices = new float[pontosLista.Count * 3];
      int ptoLista = 0;
      for (int i = 0; i < vertices.Length; i += 3)
      {
        vertices[i] = (float)pontosLista[ptoLista].X;
        vertices[i + 1] = (float)pontosLista[ptoLista].Y;
        vertices[i + 2] = (float)pontosLista[ptoLista].Z;
        ptoLista++;
      }
      bBox.Atualizar(matriz, pontosLista);

      GL.PointSize(primitivaTamanho);

      _vertexBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
      GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
      _vertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(_vertexArrayObject);
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);
    }

    public void Desenhar()
    {
#if CG_OpenGL && !CG_DirectX
      GL.PointSize(primitivaTamanho);

      GL.BindVertexArray(_vertexArrayObject);
      _shaderObjeto.Use();
      GL.DrawArrays(primitivaTipo, 0, pontosLista.Count);
#elif CG_DirectX && !CG_OpenGL
      Console.WriteLine(" .. Coloque aqui o seu código em DirectX");
#elif (CG_DirectX && CG_OpenGL) || (!CG_DirectX && !CG_OpenGL)
      Console.WriteLine(" .. ERRO de Render - escolha OpenGL ou DirectX !!");
#endif
      for (var i = 0; i < objetosLista.Count; i++)
      {
        objetosLista[i].Desenhar();
      }
    }

    #region Objeto: CRUD

    public void FilhoAdicionar(Objeto filho)
    {
      this.objetosLista.Add(filho);
    }

    public Ponto4D PontosId(int id)
    {
      return pontosLista[id];
    }

    public void PontosAdicionar(Ponto4D pto)
    {
      pontosLista.Add(pto);
      ObjetoAtualizar();
    }

    public void PontosAlterar(Ponto4D pto, int posicao)
    {
      pontosLista[posicao] = pto;
      ObjetoAtualizar();
    }

    #endregion

    #region Objeto: Grafo de Cena

    public Objeto GrafocenaBusca(char _rotulo)
    {
      if (rotulo == _rotulo)
      {
        return this;
      }
      foreach (var objeto in objetosLista)
      {
        var obj = objeto.GrafocenaBusca(_rotulo);
        if (obj != null)
        {
          return obj;
        }
      }
      return null;
    }

    public void GrafocenaImprimir(String idt)
    {
      System.Console.WriteLine(idt + rotulo);
      foreach (var objeto in objetosLista)
      {
        objeto.GrafocenaImprimir(idt + "  ");
      }
    }

    #endregion

    private Ponto4D GetClosestPoint(Ponto4D pontoNovo){
      double menorDistancia = Double.MaxValue;
      Ponto4D pontoMenorDistancia = new Ponto4D(0,0);

      foreach (var ponto in this.pontosLista)
      {
        var distancia = Matematica.distancia(pontoNovo, ponto);
        if (distancia < menorDistancia)
        {
          menorDistancia = distancia;
          pontoMenorDistancia = ponto;
        }
      }
      return pontoMenorDistancia;
    }

    public void MoveClosestPointTo(Ponto4D pontoNovo)
    {
      if (this.pontosLista.Count < 1)
        return;

      
      int indiceNovoPonto = this.pontosLista.IndexOf(GetClosestPoint(pontoNovo));
      this.pontosLista[indiceNovoPonto] = pontoNovo;
      ObjetoAtualizar();
    }

    public void RemoveClosestPointTo(Ponto4D pontoNovo){
      this.pontosLista.Remove(GetClosestPoint(pontoNovo));
      ObjetoAtualizar();
    }

    public bool Dentro(Ponto4D ponto){
      if(bBox.Dentro(ponto)){
        int interseccoes = 0;
        for (int i = 0; i < pontosLista.Count; i++)
        {
          Ponto4D ponto1 = pontosLista[i];
          Ponto4D ponto2 = i == pontosLista.Count - 1 ? pontosLista[0] : pontosLista[i+1];
          if(Matematica.ScanLine(ponto, ponto1, ponto2))
            interseccoes++;
        }

        return interseccoes % 2 == 1;
      }
      return false;
    }
    public void DesenharBbox(ref char rotuloAtual){
      var _shaderAmarela = new Shader("Shaders/shader.vert", "Shaders/shaderAmarela.frag");
      ObjetoBbox = new Retangulo(this,ref rotuloAtual, new Ponto4D(bBox.obterMenorX, bBox.obterMenorY), new Ponto4D(bBox.obterMaiorX, bBox.obterMaiorY));
      ObjetoBbox.shaderObjeto = _shaderAmarela;
      ObjetoBbox.PrimitivaTipo = PrimitiveType.LineLoop;
      ObjetoBboxCentro = new Ponto(this,ref rotuloAtual,bBox.obterCentro);
      ObjetoBboxCentro.shaderObjeto = _shaderAmarela;
      ObjetoAtualizar();
    }

    public void ApagarBbox(){
      if(ObjetoBbox != null && ObjetoBboxCentro != null){
        objetosLista.Remove(ObjetoBbox);
        ObjetoBbox.OnUnload();
        objetosLista.Remove(ObjetoBboxCentro);
        ObjetoBboxCentro.OnUnload();
      }
      
    }

    public void AplicarTranslacao(double tx, double ty, double tz = 0){
      matriz.AtribuirTranslacao(tx,ty,tz);
      AplicarMatriz();
    }

    public void AplicarEscala(double sx, double sy, double sz = 0){
      matriz.AtribuirEscala(sx,sy,sz);
      AplicarMatriz();
    }

    public void AplicarRotacaoZ(double angulo){
      bBox.ProcessarCentro();
      var centro = new Ponto4D(bBox.obterCentro.X, bBox.obterCentro.Y);
      AplicarTranslacao(0 - centro.X, 0 - centro.Y, 0 - centro.Z);
      matriz.AtribuirRotacaoZ(angulo);
      AplicarMatriz();

      AplicarTranslacao(centro.X,centro.Y,centro.Z);
    }

    private void AplicarMatriz(){
      ApagarBbox();
      for (int i = 0; i < pontosLista.Count; i++)
      {
        pontosLista[i] = matriz.MultiplicarPonto(pontosLista[i]);
      }
      ObjetoAtualizar();
    }

    public void OnUnload()
    {
      foreach (var objeto in objetosLista)
      {
        objeto.OnUnload();
      }

      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.BindVertexArray(0);
      GL.UseProgram(0);

      GL.DeleteBuffer(_vertexBufferObject);
      GL.DeleteVertexArray(_vertexArrayObject);

      GL.DeleteProgram(_shaderObjeto.Handle);
    }

#if CG_Debug
    protected string ImprimeToString()
    {
      string retorno;
      retorno = "__ Objeto: " + rotulo + "\n";
      for (var i = 0; i < pontosLista.Count; i++)
      {
        retorno += "P" + i + "[ " +
        string.Format("{0,10}", pontosLista[i].X) + " | " +
        string.Format("{0,10}", pontosLista[i].Y) + " | " +
        string.Format("{0,10}", pontosLista[i].Z) + " | " +
        string.Format("{0,10}", pontosLista[i].W) + " ]" + "\n";
      }
      retorno += bBox.ToString();
      return (retorno);
    }
#endif

  }
}