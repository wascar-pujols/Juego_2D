// Archivo generado: mapas de los niveles (cada caracter es un tile de 1x1).
// Leyenda: # suelo  S bloque de piedra  B caja rompible  ? bloque moneda  h bloque corazon
//  o moneda  H corazon  ^ pinchos  R ronin  N shinobi  Y yokai-farol  < > torreta de shuriken
//  F barra de fuego  = puente que se cae  M plataforma movil horizontal  V vertical
//  W plataforma de madera (un solo sentido)  K checkpoint  P jugador  G meta (torii)  A inicio de arena del jefe  X jefe  L farol decorativo
public static class LevelData
{
    public static readonly string[] Names = {
        "Aldea del Bambú",
        "Paso de la Montaña Kaze",
        "Castillo del Shogun",
    };

    public static readonly int[] TimeLimit = { 240, 260, 300 };

    public static readonly string[][] Maps = {
        new string[] {
            "                                                                                                                                                                                                        ",
            "                                                                                                                                                                                                        ",
            "                                                                                                                                                                                                        ",
            "                                                                                                                                                                                                        ",
            "                                                                                                                      ooo                                                       ooooo                   ",
            "              ?                                                                                                      SSSSS                                                                              ",
            "                                                                                                                                                                                                        ",
            "                                                                                                                    Y                                                              SSS                  ",
            "                                              Y  BB?BB        oo                    ooooo                                              o                              Y           SSSS                  ",
            "            ?BhB?              ooo         o                                  ??                            Y         B?B              S                      ?h?                SSSSS                  ",
            "                                        S o o                 SS         <                       oooooo                               SS                                        SSSSSS                  ",
            "                                       SS                                S                                                           SSS       M                               SSSSSSS                  ",
            " L P    ooo           R               SSS         ^^^    R          L   SS         R    R   K                   ^^          R   ^^^ SSSS   R            L  >      R      ^^   SSSSSSSS        G     L   ",
            "###############################   ###########################     ##############################========#####################################         ##################################################",
            "###############################   ###########################     ##############################        #####################################         ##################################################",
        },
        new string[] {
            "                                                                                                                                                                                                                       ",
            "                                                                                                                                                                                                                       ",
            "                                                                                                                                                                                                                       ",
            "                                                                                                                                                                                                                       ",
            "                                                                                                                                             oo                                                                        ",
            "                                                            ooo                     Y                                                        SSS                                                                       ",
            "                                                  ooo                                                                                                                                                                  ",
            "                                                                                                                                         SS        SS                     Y                                            ",
            "                        ooo    F                                                B?B?B                            F  oooooo                        Y                                       F                            ",
            "         ?B?                                            V                   <                                                    S   SS                SS         ?h?                                   S              ",
            "                                                M             M             S                                                   SS                                                                     SS              ",
            "                                      R  ^^                                SS                                                  SSS                                                                    SSS              ",
            "   P             N         ###################                        N   SSS         ^^^ K             N     R      ^^^^   N SSSS                           >    N  R                        ^^  N  SSSS      G       ",
            "#######################    ###################                      #########################=======###############################                        #####################=======################################",
            "#######################    ###################                      #########################       ###############################                        #####################       ################################",
        },
        new string[] {
            "                                                                                                                                                                                             S",
            "                                                                                                                                                                                             S",
            "                                                                                                                                                                                             S",
            "                                                                                                                                                                                             S",
            "                                                                                                                                                                                             S",
            "                                                                      ooooo                                                                                                                  S",
            "                                                                                                                                                                                             S",
            "                                                                                      Y                                                                                                      S",
            "                      F  ooo             Y                          F       F                                           Y         F                                        h                 S",
            "        ?h?                                                                                                  B?B                      h                             WWW          WWW         S",
            "                                                 S    M                                                                                                                                      S",
            "                                    S           SS                                                                                                                                           S",
            " L P          R    N           ^^^  <         NSSS             R       ^^^^       N       K             >        R   N      ^^^           N     L  K       A                          X      S",
            "#########################   #######################          ################################========#########################################################################################",
            "#########################   #######################          ################################        #########################################################################################",
        },
    };
}
