using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using SFML.Audio;

namespace SFMLApp
{   // Nummerdatud "objekti tüübid" normal=0, eatable=1, exit=2 ...
    public enum otype { normal, eatable, exit }

    // Mängija liikumis suunad
    public enum Direction
    {
        None = 0,
        North = 1,
        NorthEast = 3,
        East = 2,
        SouthEast = 6,
        South = 4,
        SouthWest = 12,
        West = 8,
        NorthWest = 9
    }

    // klassi ujuvkoma Punkt definitsioon
    public class Point2f
    {
        public float x, y;
        public Point2f(float pX, float pY) { Set(pX, pY); }
        void Set(float pX, float pY) { x = pX; y = pY; }
    }

    // Mängu objekt, Kasuatatkse staatiliste objektide jaoks
    // koordinaadid arvestatakse vasakust ülemisest nurgast, laiesu ja kõrgusega
    // koosneb RectangleShape joonistamise infoga

    public class GameObject : Drawable
    {
        protected float x, y, w, h;
        public RectangleShape Rect { get; private set; }
        public otype oType { get; private set; }  // objekti tüüp tavaline, ja kustutav
        public int Score = 0;  // punktid mida lisatakse
        public float X { get => x; set { x = value; Rect.Position = new Vector2f(x, Rect.Position.Y); } }
        public float Y { get => y; set { y = value; Rect.Position = new Vector2f(Rect.Position.X, y); } }
        public float W { get => w; set { w = value; Rect.Size = new Vector2f(value, Rect.Size.Y); } }
        public float H { get => h; set { h = value; Rect.Size = new Vector2f(Rect.Size.X, value); } }

        public GameObject(float pX, float pY, float pW, float pH, Texture txtr = null, otype tp = otype.normal, int scr = 0)
        {
            x = pX; y = pY; w = pW; h = pH;
            Rect = new RectangleShape(new Vector2f(w, h));
            Rect.Position = new Vector2f(pX, pY);
            Rect.Texture = txtr; oType = tp; Score = scr;
        }

        // Teise objektiga ristumise(collision) kontroll
        public bool Intersects(GameObject obj)
        {
            return (this.x <= (obj.x + obj.w) && this.x + w >= obj.x) && (this.x <= (obj.x + obj.w) && ((this.x + w) > obj.x));
        }

        // Teise objektiga ristumis kontroll kasutades etteantud koordinate
        public bool Intersects(float pX, float pY, float pW, float pH)
        {
            return (this.x <= (pX + pW) && this.x + w >= pX) && (this.y <= (pY + pH) && ((this.y + h) > pY));
        }
        // Ristumine(kollisiooni) suuna määramine

        public Direction CollisionDir(GameEntity obj)
        {
            Direction result = Direction.None;

            // Saame proetseeritud punkti, kuhu on vaja liigutada objekt
            Point2f MoveTo = obj.MoveProject(obj.Speed);
            // Kontrollime kokkupõrget olemasoleva ja liigutava objekti vahel
            bool collides = Intersects(MoveTo.x, MoveTo.y, obj.W, obj.H);
            // Kui kokkupõrget ei ole, tagastame 0 suuna (Direction.None) 
            if (!collides) return Direction.None;

            float minX = this.X;
            float maxX = this.X + this.W;
            float minY = this.Y;
            float maxY = this.Y + this.H;

            // Lejame objektide keskpunktid
            Point2f midpoint_this = new Point2f(this.X + (this.W / 2), this.Y + (this.H / 2));
            Point2f midpoint_other = new Point2f(obj.X + (obj.W / 2), obj.Y + (obj.H / 2));

            // Lejame vektori/differendi kahe objekti vahel
            double dx = midpoint_other.x - midpoint_this.x;
            double dy = midpoint_other.y - midpoint_this.y;

            // Kui dx moodul on suurem dy moodulist, siis on horisontaalne suund vasakult paremale
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                // Kui dx >= 0, suund on vasakule (west)
                if (dx >= 0) result = Direction.West;
                // Vastasel juhul paremale (east)
                else result = Direction.East;
            }
            // Kui  dy moodul on suurem, kollisioon on vertikaalne 
            else
            {
                if (dy > 0) result = Direction.North;
                else result = Direction.South;
            }

            return result;
        }

        // Metod mis pärineb classist Drawable, sellecks et GameObject võiks olla joonistatud ekraanile kasutades meetodit window.Draw(obj);
        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(Rect, states);
        }
    }

    // Baas klass TehisIntellekti "AI loomiseks"
    public abstract class EntityAI
    {
        protected GameEntity entity;
        protected GameWorld world;

        public EntityAI(GameEntity p_entity, GameWorld p_world)
        {
            entity = p_entity; // TI võimalused
            world = p_world; // mänguruum
        }

        // TehisIntellekti sammhaaval strateegia üks käik
        public abstract void Tick();
    }


    // Selle klassi objekt omab kiirust, suunda ja AI(tarkust)
    public class GameEntity : GameObject
    {
        public const float Sqrt2 = 1.41421356f;

        public float Speed = 0.0f;
        public Direction Dir = Direction.None;

        public EntityAI AI = null;

        public GameEntity(float pX, float pY, float pW, float pH, Texture txtr, otype tp, int scr) : base(pX, pY, pW, pH, txtr, tp, scr) { }

        // Сгенерировать точку, в которую будем двигаться с направлением this.Dir и указанной скоростью
        public Point2f MoveProject(float pSpeed)
        {
            float moveToX = 0.0f;
            float moveToY = 0.0f;

            switch (Dir)
            {
                case Direction.None:
                    moveToX = this.x;
                    moveToY = this.Y;
                    break;
                case Direction.North:
                    // x не меняется - только y против оси x (в экранных координатах направлена вниз)
                    moveToX = this.x;
                    moveToY = this.y - pSpeed;
                    break;
                case Direction.NorthEast:
                    // меняется и x и y, угол 45 градусов, так что движение поровно, делённое на корень из двух
                    moveToX = this.x + pSpeed / Sqrt2;
                    moveToY = this.y - pSpeed / Sqrt2;
                    break;
                case Direction.East:
                    moveToX = this.x + pSpeed;
                    moveToY = this.y;
                    break;
                case Direction.SouthEast:
                    moveToX = this.x + pSpeed / Sqrt2;
                    moveToY = this.y + pSpeed / Sqrt2;
                    break;
                case Direction.South:
                    moveToX = this.x;
                    moveToY = this.y + pSpeed;
                    break;
                case Direction.SouthWest:
                    moveToX = this.x - pSpeed / Sqrt2;
                    moveToY = this.y + pSpeed / Sqrt2;
                    break;
                case Direction.West:
                    moveToX = this.x - pSpeed;
                    moveToY = this.y;
                    break;
                case Direction.NorthWest:
                    moveToX = this.x - pSpeed / Sqrt2;
                    moveToY = this.y - pSpeed / Sqrt2;
                    break;
            }

            return new Point2f(moveToX, moveToY);
        }

        // Враппер для текущей скорости сущности
        public bool MoveEntity(GameWorld w, bool checkonly = false)
        {
            return MoveEntity(w, Speed, out Direction d, checkonly);
        }

        // Попробовать движение в указанном мире с указанной скоростью
        // Если выставлен "checkonly" - только делаем проверку, не пытаемся передвигаться
        public bool MoveEntity(GameWorld w, float pSpeed, out Direction bumpDirection, bool checkonly = false)
        {
            bumpDirection = Direction.None;
            if (pSpeed <= 0.0f) return false;


            Point2f moveTo = MoveProject(pSpeed);

            // Проверяем коллизию и её направление со спроецированными координатами в указанном игровом мире
            bool canMove = w.CheckCollision(this, moveTo.x, moveTo.y, this.W, this.H, out bumpDirection);

            // Если коллизии нет - производим перемещение
            if (canMove)
            {
                this.X = moveTo.x;
                this.Y = moveTo.y;
                return true;
            }
            return false;
        }

        // Если AI объекта обозначен - вызываем сеанс его "мышления"
        public void AITick()
        {
            if (AI != null) AI.Tick();
        }
    }

    // Klass "mänguruum"
    // Omab viiteid applikatsioonile, mänguruumile, mängija figuurile, kõikide staatilistele objektideleс
    // mänguruum - on kah mäggu objekt
    public class GameWorld
    {
        public GameApp app; // applikatsioon (tehnilised asjad, sisend/väljund, joonistamine)
        public GameObject world;  // mänguruum
        public GameEntity player; // liigutatav klahvide abil mängija figuur
        public List<GameObject> statics = new List<GameObject>(); // Staatiliste objektide list
        public List<GameEntity> entities = new List<GameEntity>();  // Liikuvate objektide list
        public int Score; // Mängus kogutud punktide arv
        public bool finish = false;
        public static bool end = false;
        public int countBalls = 0;
        // Mänguruumi initsialisaator, see kutsutakse välja mänguruumi loomisel
        public GameWorld(GameApp p_app, float WorldW, float WorldH, float PlayerW, float PlayerH)
        {
            app = p_app;
            world = new GameObject(0, 0, WorldW, WorldH, null, 0, 0); // Loome mänguruumi suurusega WorldW * WorldH
            player = new GameEntity(64, 64, PlayerW, PlayerH, null, 0, 0); // Määrame mängija figuuri algpunkti
            entities.Add(player); // Lisame mängija liikuvate objektide listi
            Score = 0;
        }

        // Kontrollime antud liikuva objekti kokkupõrget mämguruumi piiridega ja kõikide objektidega mänguruumis
        public bool CheckCollision(GameEntity entity, float x, float y, float pW, float pH, out Direction bumpDirection)
        {
            bumpDirection = Direction.None;
            // Check world boundaries collision
            bool worldResult = x > world.X && (x + pW) < (world.X + world.W) && y > world.Y && (y + pH) < (world.Y + world.H);

            if (!worldResult)
            {
                // Если есть коллизия с границами мира, определяем направление (где объект выходит за рамки)
                if (x < world.X) bumpDirection |= Direction.West;
                else if (x + pW > (world.X + world.W)) bumpDirection |= Direction.East;
                else if (y + pH > (world.Y + world.H)) bumpDirection |= Direction.South;
                else if (y < world.Y) bumpDirection |= Direction.North;
            }
            // Kontrollime antud liikuva objekti kokkupõrget kõikide staatiliste objektidega mänguruumis, peatume esimesel kokkupõrkel
            bool staticsResult = true;
            GameObject temp = null;  // loome muutuja leitud kokkupõrke objekti meeldejätmiseks
            foreach (GameObject stat in statics)
            {
                // Alguses kontrollime kokkupõrget
                bool r = stat.Intersects(x, y, pW, pH);
                if (r == true)
                {
                    staticsResult = false;
                    // Kui kokkupõrge leiti, tuleb leida tema suund
                    bumpDirection = stat.CollisionDir(entity);
                    if (stat.oType == otype.eatable) // Kui kokkupõrke object on söödav
                    {
                        Score += stat.Score; // Lisame punktid objektist üld scoori
                        temp = stat;  // teeme ära söödud objectist ajutise koopia kustutamiseks
                        countBalls++;
                        //MessageBox.Show("hkbvkhgoyhu");
                    }


                    /*else if (stat.oType == otype.exit)
                    {
                        finish = true;
                        expl = true;
                    }*/
                    break; // lõpetame listi töötlemise
                }
            }
            // Kustutame ära söödud objekti
            if (temp != null) statics.Remove(temp);

            // Sama töötlus liikuvate objektidega
            bool entitiesResult = true;
            foreach (GameEntity ent in entities)
            {
                if (entity == ent) continue;
                bool r = ent.Intersects(x, y, pW, pH);
                if (r == true)
                {
                    entitiesResult = false;
                    bumpDirection = ent.CollisionDir(entity);

                    break;
                }
            }

            // Tagastame kõigi kolme kontrolli loogilise "ja" resultaadi
            return worldResult && staticsResult && entitiesResult;
        }
    }

    public class GameApp
    {
        // Määrame mängu akna suuruse
        public const int W = 1024, H = 786;
        public const int StatusBarH = 80;
        public const int Framerate = 60;
        String Title = "Korja palle, sinu punktid: ";

        RenderWindow window;
        Vector2f center;

        GameWorld world;
        Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
        const int pW = 40, pH = 60;

        RectangleShape cursor;
        SFML.Graphics.Font sensation = new Font("Sansation_Bold.ttf");

        public Stopwatch clock { get; private set; } = Stopwatch.StartNew();

        public GameApp()
        {
            // Loome akna
            window = new RenderWindow(new VideoMode(W, H + StatusBarH, 32), "Game app",
                Styles.Close | Styles.Titlebar );

            // Registreerime sündmuste töötlejad: applikatsiooni lõpetamiseks ja klahvi vajutuseks
            window.Closed += new EventHandler(WindowClosed);
            window.KeyPressed += new EventHandler<SFML.Window.KeyEventArgs>(KeyPressed);


            //SFML.Graphics.Font sensation = new Font("Sansation_Bold.ttf");



            center = new Vector2f(W / 2.0f, H / 2.0f);

            // Laeme ja registreerime kasutusel olevad textuurid nendele eraldatud nimedega
            textures.Add("player", new Texture("player.png"));
            textures.Add("ball", new Texture("ball.png"));
            textures.Add("rock", new Texture("rock.png"));
            textures.Add("wall", new Texture("brick.png"));

            // Loome mängumaailma
            world = new GameWorld(this, W, H, pW, pH);
        }

        public void Init()
        {
            //window.SetMouseCursorVisible(false);

            //// Lisame staatilised objektid
            ////List<GameObject> items = new List<GameObject>(); // loome staatiliste objektide listi
            //for (int i = 0; i < 15; i++)
            //{
            //    if (i < 12)
            //    {
            //        world.statics.Add(new GameObject(0, 64 * i, 64, 64, textures["wall"], 0, 0));
            //    }
            //    world.statics.Add(new GameObject(64 * i, 0, 64, 64, textures["wall"], 0, 0));
            //    world.statics.Add(new GameObject(64 * i, 720, 64, 64, textures["wall"], 0, 0));
            //    //if (i < 14)
            //    //{
            //    //    world.statics.Add(new GameObject(64 * i, 128, 64, 64, textures["wall"], 0, 0));
            //    //}
            //    //if (!(i > 13))
            //    //{
            //    //    world.statics.Add(new GameObject(960, 64 * i, 64, 64, textures["wall"], 0, 0));
            //    //}
            //    //if (i != 0 && i != 1)
            //    //{
            //    //    world.statics.Add(new GameObject(64 * i, 256, 64, 64, textures["wall"], 0, 0));
            //    //}
            //    //if (i != 7 && i != 8)
            //    //{
            //    //    world.statics.Add(new GameObject(64 * i, 384, 64, 64, textures["wall"], 0, 0));
            //    //}
            //    //if (i != 11)
            //    //{
            //    //    world.statics.Add(new GameObject(64 * i, 515, 64, 64, textures["wall"], 0, 0));
            //    //}
            //    //if (i % 2 == 0)
            //    //{
            //    //    world.statics.Add(new GameObject(64 * i, 576, 64, 64, textures["wall"], 0, 0)); //teine kõrgus

            //    //}
            //}
            //world.statics.Add(new GameObject(W - 178, H - 192, 32, 32, textures["ball"], otype.eatable, 10));
            //world.statics.Add(new GameObject(W - 560, H - 192, 32, 32, textures["ball"], otype.eatable, 10));
            //world.statics.Add(new GameObject(80, H - 192, 32, 32, textures["ball"], otype.eatable, 10));
            //world.player.X = 2;
            //world.player.Y = 2;
            //world.Score = 0;
        }

        public bool Run()
        {
            // Mängija initsialiseerimine
            world.player.Speed = 5.0f;
            world.player.Rect.Texture = textures["player"];
            // Peidame kursori ära 
            window.SetMouseCursorVisible(false);

            // Lisame staatilised objektid
            //List<GameObject> items = new List<GameObject>(); // loome staatiliste objektide listi
            for (int i = 0; i < 15; i++)
            {
                if (i < 12)
                {
                    world.statics.Add(new GameObject(0, 64 * i, 64, 64, textures["wall"], 0, 0));
                }
                world.statics.Add(new GameObject(64 * i, 0, 64, 64, textures["wall"], 0, 0));
                world.statics.Add(new GameObject(64 * i, 720, 64, 64, textures["wall"], 0, 0));
                if (i < 14)
                {
                    world.statics.Add(new GameObject(64 * i, 128, 64, 64, textures["wall"], 0, 0));
                }
                if (!(i > 13))
                {
                    world.statics.Add(new GameObject(960, 55 * i, 64, 64, textures["wall"], 0, 0));
                }
                if (i != 0 && i != 1)
                {
                    world.statics.Add(new GameObject(64 * i, 256, 64, 64, textures["wall"], 0, 0));
                }
                if (i != 7 && i != 8)
                {
                    world.statics.Add(new GameObject(64 * i, 384, 64, 64, textures["wall"], 0, 0));
                }
                if (i != 11)
                {
                    world.statics.Add(new GameObject(64 * i, 515, 64, 64, textures["wall"], 0, 0));
                }
                if (i % 2 == 0)
                {
                    world.statics.Add(new GameObject(64 * i, 576, 64, 64, textures["wall"], 0, 0)); //teine kõrgus

                }
            }
            world.statics.Add(new GameObject(W - 178, H - 192, 32, 32, textures["ball"], otype.eatable, 10));
            world.statics.Add(new GameObject(W - 560, H - 192, 32, 32, textures["ball"], otype.eatable, 10));
            world.statics.Add(new GameObject(80, H - 192, 32, 32, textures["ball"], otype.eatable, 10));

            cursor = new RectangleShape(new Vector2f(10, 10));

            // Programmi peatsükkel
            // 1. Sündmuste töötlemine
            // 2. Mängu loogika implementeerimine
            // 3. Joonistamine/graafiliste objektide väljastamine ja muu 
            while (window.IsOpen)
            {
                // Kutsub välja sündmuste töötlemise
                window.DispatchEvents();
                /*if (world.finish == true)
                {
                    window.Close();
                }*/

                SFML.Graphics.Text endtext = new Text("Mäng läbi", sensation);
                // Akna alla positsioneerimine
                endtext.Position = new Vector2f((W / 2) - 128, (H / 2) - 128);
                // Fondi suuruse määramine
                endtext.CharacterSize = 80;
                endtext.FillColor = Color.Red;
                endtext.OutlineColor = Color.Black;
                endtext.OutlineThickness = 10;

                // Muudab mängija asukohta
                world.player.Dir = CalculateDirection();
                world.player.MoveEntity(world);

                SFML.Graphics.Text scrtext = new Text(Title + world.Score, sensation);
                // Akna alla positsioneerimine
                scrtext.Position = new Vector2f(0.0f, H + 2.0f);
                // Fondi suuruse määramine
                scrtext.CharacterSize = 40;

                // Positsioneerime kursori
                Vector2i mpos = SFML.Window.Mouse.GetPosition(window);
                cursor.Position = new Vector2f(mpos.X, mpos.Y);

                // Täidame ekraani rohelise värviga
                window.Clear(new Color(0x30, 0x90, 0x30, 0xff));

                // Staatiliste ja liikuvate objektide väljastamine ja vajadusel AI toimetamine
                foreach (GameObject stat in world.statics) window.Draw(stat);
                foreach (GameEntity entity in world.entities)
                {
                    entity.AITick();
                    window.Draw(entity);
                }
                // Joonistame mängija figuuri
                window.Draw(world.player);
                // Joonistame kursori
                window.Draw(cursor);

                if (world.countBalls == 3)
                {
                    //window.Close();
                    window.Draw(endtext);
                }

                window.Draw(scrtext);
                // Näitame ekraanile
                window.Display();

                // Ootame veidi järgmise joonistamis momendini (60hz)
                Thread.Sleep(1000 / Framerate);
            }
            return false;
        }

        // Määrame mängija figuuri liikumise suunda klahvide vajutuse järgi
        Direction CalculateDirection()
        {
            Direction dir = Direction.None;
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.W)) dir |= Direction.North;
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.S)) if (dir.HasFlag(Direction.North)) dir -= Direction.North; else dir |= Direction.South;
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.A)) dir |= Direction.West;
            if (SFML.Window.Keyboard.IsKeyPressed(Keyboard.Key.D)) if (dir.HasFlag(Direction.West)) dir -= Direction.West; else dir |= Direction.East;

            return dir;
        }
        //reinit
        void WindowClosed(object sender, EventArgs e)
        {
            if (window != null) window.Close();
        }

        void KeyPressed(object sender, SFML.Window.KeyEventArgs e)
        {
            float changeTo = 0;

            switch (e.Code)
            {
                case Keyboard.Key.Escape:
                    window.Close();
                    break;
                case Keyboard.Key.BackSpace:
                    world.player.X = world.world.W / 2 - world.player.W / 2;
                    world.player.Y = world.world.H / 2 - world.player.H / 2;
                    break;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            GameApp application = new GameApp();
            while (application.Run()) ;
            //return; 
            if (GameWorld.end == true)
            {
                application.Run();
            }
        }
    }
}
