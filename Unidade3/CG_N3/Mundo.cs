#define CG_Gizmo  // debugar gráfico.
#define CG_OpenGL // render OpenGL.
//#define CG_DirectX // render DirectX.
//#define CG_Privado // código do professor.

using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

//FIXME: padrão Singleton

namespace gcgcg
{
    public class Mundo : GameWindow
    {
        Objeto mundo;
        private char rotuloAtual = '?';
        private List<Objeto> objetosMundo = new List<Objeto>();
        private Objeto objetoSelecionado = null;

        private readonly float[] _sruEixos =
        {
       -0.5f,  0.0f,  0.0f, /* X- */      0.5f,  0.0f,  0.0f, /* X+ */
       0.0f,  -0.5f,  0.0f, /* Y- */      0.0f,  0.5f,  0.0f, /* Y+ */
       0.0f,  0.0f,  -0.5f, /* Z- */      0.0f,  0.0f,  0.5f, /* Z+ */
    };

        private int _vertexBufferObject_sruEixos;
        private int _vertexArrayObject_sruEixos;

        private Shader _shaderVermelha;
        private Shader _shaderVerde;
        private Shader _shaderAzul;

        private bool mouseMovtoPrimeiro = true;
        private Ponto4D mouseMovtoUltimo;
        private Vector2 _lastPos;
        private bool ehCriacaoPoligono = false;

        public Mundo(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
               : base(gameWindowSettings, nativeWindowSettings)
        {
            mundo = new Objeto(null, ref rotuloAtual);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            #region Eixos: SRU  
            _vertexBufferObject_sruEixos = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject_sruEixos);
            GL.BufferData(BufferTarget.ArrayBuffer, _sruEixos.Length * sizeof(float), _sruEixos, BufferUsageHint.StaticDraw);
            _vertexArrayObject_sruEixos = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject_sruEixos);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            _shaderVermelha = new Shader("Shaders/shader.vert", "Shaders/shaderVermelha.frag");
            _shaderVerde = new Shader("Shaders/shader.vert", "Shaders/shaderVerde.frag");
            _shaderAzul = new Shader("Shaders/shader.vert", "Shaders/shaderAzul.frag");
            #endregion

#if CG_Privado
             #region Objeto: circulo  
             objetoSelecionado = new Circulo(mundo, ref rotuloAtual, 0.2, new Ponto4D());
             objetoSelecionado.shaderObjeto = new Shader("Shaders/shader.vert", "Shaders/shaderAmarela.frag");
             #endregion

             #region Objeto: SrPalito  
             objetoSelecionado = new SrPalito(mundo, ref rotuloAtual);
             #endregion

             #region Objeto: Spline
             objetoSelecionado = new Spline(mundo, ref rotuloAtual);
             #endregion
#endif

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

#if CG_Gizmo
            Sru3D();
#endif
            mundo.Desenhar();
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var keyboardInput = KeyboardState;

            var mouseInput = MouseState;
            int janelaLargura = Size.X;
            int janelaAltura = Size.Y;
            Ponto4D mousePonto = new Ponto4D(MousePosition.X, MousePosition.Y);
            Ponto4D sruPonto = Utilitario.NDC_TelaSRU(janelaLargura, janelaAltura, mousePonto);

            #region Teclado

            if(keyboardInput.IsKeyPressed(Keys.Enter) && ehCriacaoPoligono){
                ehCriacaoPoligono = false;
                objetosMundo.Add(objetoSelecionado);
            }
            if(objetoSelecionado != null){
                if(keyboardInput.IsKeyPressed(Keys.D)){
                    objetosMundo.Remove(objetoSelecionado);
                    objetoSelecionado.OnUnload();
                }

                if(keyboardInput.IsKeyDown(Keys.V)){
                    objetoSelecionado.MoveClosestPointTo(sruPonto);
                    objetoSelecionado.ApagarBbox();
                    objetoSelecionado.DesenharBbox(ref rotuloAtual);
                }

                if(keyboardInput.IsKeyPressed(Keys.E)){
                    objetoSelecionado.RemoveClosestPointTo(sruPonto);
                    objetoSelecionado.ApagarBbox();
                    objetoSelecionado.DesenharBbox(ref rotuloAtual);
                }

                if(keyboardInput.IsKeyPressed(Keys.P)){
                    if(objetoSelecionado.PrimitivaTipo == PrimitiveType.LineLoop)
                        objetoSelecionado.PrimitivaTipo = PrimitiveType.LineStrip;
                    else{
                        objetoSelecionado.PrimitivaTipo = PrimitiveType.LineLoop;
                    }
                    objetoSelecionado.ObjetoAtualizar();
                }

                if(keyboardInput.IsKeyPressed(Keys.R)){
                    objetoSelecionado.shaderObjeto = _shaderVermelha;
                }
                if(keyboardInput.IsKeyPressed(Keys.G)){
                    objetoSelecionado.shaderObjeto = _shaderVerde;
                }
                if(keyboardInput.IsKeyPressed(Keys.B)){
                    objetoSelecionado.shaderObjeto = _shaderAzul;
                }

                if(keyboardInput.IsKeyPressed(Keys.Up)){
                    objetoSelecionado.AplicarTranslacao(0,0.5);
                }
                if(keyboardInput.IsKeyPressed(Keys.Down)){
                    objetoSelecionado.AplicarTranslacao(0,-0.5);
                }
                if(keyboardInput.IsKeyPressed(Keys.Left)){
                    objetoSelecionado.AplicarTranslacao(-0.5,0);
                }
                if(keyboardInput.IsKeyPressed(Keys.Right)){
                    objetoSelecionado.AplicarTranslacao(0.5,0);
                }

                if(keyboardInput.IsKeyPressed(Keys.Home)){
                    objetoSelecionado.AplicarEscala(0.5,0.5);
                }
                if(keyboardInput.IsKeyPressed(Keys.End)){
                    objetoSelecionado.AplicarEscala(-0.5,-0.5);
                }

                if(keyboardInput.IsKeyPressed(Keys.D3)){
                    objetoSelecionado.AplicarRotacaoZ(15.0);
                }
                if(keyboardInput.IsKeyPressed(Keys.D4)){
                    objetoSelecionado.AplicarRotacaoZ(-15.0);
                }
            }
            #endregion

            #region  Mouse

            if(mouseInput.IsButtonPressed(MouseButton.Button2)){
                if(!ehCriacaoPoligono){
                    if(objetoSelecionado != null)
                        objetoSelecionado.ApagarBbox();
                    ehCriacaoPoligono = true;
                    objetoSelecionado = new Poligono(mundo, ref rotuloAtual, new List<Ponto4D>(){sruPonto});
                    objetoSelecionado.PrimitivaTipo = PrimitiveType.LineLoop;
                }
                else{
                    objetoSelecionado.PontosAdicionar(sruPonto);
                }
            }

            if(mouseInput.IsButtonDown(MouseButton.Button2)){
                if(!mouseInput.IsButtonPressed(MouseButton.Button2))
                    objetoSelecionado.MoveClosestPointTo(sruPonto);
            }

            if(mouseInput.IsButtonPressed(MouseButton.Button1)){
                objetoSelecionado.ApagarBbox();
                foreach (var objeto in objetosMundo)
                {
                    if(objeto.Dentro(sruPonto)){
                        objetoSelecionado = objeto;
                        objetoSelecionado.DesenharBbox(ref rotuloAtual);
                        break;
                    }
                }
            }
        }
        #endregion

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            mundo.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject_sruEixos);
            GL.DeleteVertexArray(_vertexArrayObject_sruEixos);

            GL.DeleteProgram(_shaderVermelha.Handle);
            GL.DeleteProgram(_shaderVerde.Handle);
            GL.DeleteProgram(_shaderAzul.Handle);

            base.OnUnload();
        }

#if CG_Gizmo
        private void Sru3D()
        {
#if CG_OpenGL && !CG_DirectX
            GL.BindVertexArray(_vertexArrayObject_sruEixos);
            // EixoX
            _shaderVermelha.Use();
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            // EixoY
            _shaderVerde.Use();
            GL.DrawArrays(PrimitiveType.Lines, 2, 2);
            // EixoZ
            _shaderAzul.Use();
            GL.DrawArrays(PrimitiveType.Lines, 4, 2);
#elif CG_DirectX && !CG_OpenGL
      Console.WriteLine(" .. Coloque aqui o seu código em DirectX");
#elif (CG_DirectX && CG_OpenGL) || (!CG_DirectX && !CG_OpenGL)
      Console.WriteLine(" .. ERRO de Render - escolha OpenGL ou DirectX !!");
#endif
        }
#endif

    }
}