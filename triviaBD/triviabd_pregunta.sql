-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: triviabd
-- ------------------------------------------------------
-- Server version	8.0.45

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `pregunta`
--

DROP TABLE IF EXISTS `pregunta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pregunta` (
  `idPregunta` int NOT NULL,
  `puntuacionPregunta` int NOT NULL,
  `nomPregunta` varchar(150) NOT NULL,
  `idCategoria` int NOT NULL,
  PRIMARY KEY (`idPregunta`),
  UNIQUE KEY `idPpregunta_UNIQUE` (`idPregunta`),
  KEY `idCategoria_idx` (`idCategoria`),
  CONSTRAINT `idCategoria` FOREIGN KEY (`idCategoria`) REFERENCES `categoria` (`idCategoria`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pregunta`
--

LOCK TABLES `pregunta` WRITE;
/*!40000 ALTER TABLE `pregunta` DISABLE KEYS */;
INSERT INTO `pregunta` VALUES (1,10,'¿Irais y lalo se han besado?',1),(2,10,'¿Quién le rompió el lavamanos a Mauricio?',1),(3,10,'¿Con quién engaño Eloy a Cons?',1),(4,10,'¿Cuántas personas tortearon en la peda del Jacob?',1),(5,10,'¿Quién ha vomitado más veces en la casa de Mauricio?',1),(6,10,'¿Cuántas veces han terminado Irais y Atrishka?',1),(7,10,'¿A quién representa el siguiente sonido?',1),(8,10,'¿Nombre de la persona que rechazo a Lalo?',1),(9,10,'Persona que casi obtiene una orden de restricción',1),(10,10,'¿Quién es el más migajero de la banca?',1),(11,10,'¿Quién tuvo la relación con la persona más tóxica?',1),(12,10,'Persona con más stickers en la banca',1),(13,10,'¿Cuál de los siguientes sonidos es producido por una marimba?',2),(14,10,'¿Qué línea de voz le pertenece al cantante Frank Sinatra?',2),(15,10,'¿Qué línea de voz le pertenece al cantante Juan Gabriel?',2),(16,10,'¿Cuál de las siguientes canciones pertenece al género de Jazz?',2),(17,10,'¿Quién es conocido como el príncipe de la canción?',2),(18,10,'¿Cuál de las siguientes cantantes es Amy Winehouse?',2),(19,10,'¿Cuál de los siguientes modelos de guitarra eléctrica es una Gibson Lee Paul?',2),(20,10,'¿Cuál de las siguientes imágenes pertenece a un contrabajo?',2),(21,10,'¿Cuál fue la primera edición del festival Woodstock?',2),(22,10,'¿Quién fue el creador de la Moonlight Sonata?',2),(23,10,'¿Cuál de las siguientes géneros fue uno de los antecedentes directos del Rock?',2),(24,10,'¿En cuál álbum fue incluida la canción \"Smeels Like Teen Spirit\" de Nirvana?',2),(25,10,'¿Cuál de los siguientes sonidos corresponde a una naracción de futbol?',3),(26,10,'¿Qué sonido marca el final de un partido de Basquetbol?',3),(27,10,'¿Qué sonido corresponde a una pelota de Tennis?',3),(28,10,'¿Cuál de las siguientes narraciones pertenece al comentarista de Futbol Cristian Martinolli?',3),(29,10,'¿Cuál de las siguientes imágenes corresponde al trofeo Vince Lombardi?',3),(30,10,'¿Cuál de los siguientes equipos es el actual campeón del mundo en futbol?',3),(31,10,'¿Quién es conocido como \"O Rei\"?',3),(32,10,'¿Qué escudería de F1 tiene más campeonatos mundiales?',3),(33,10,'¿Quién es el basquetbolista con más campeonatos de NBA ganados en su carrera?',3),(34,10,'¿Qué es un homerun?',3),(35,10,'¿Cuál fue el evento de medio tiempo más visto en la NFL? ',3),(36,10,'¿Qué equipo de la Premier League tiene más Champions ganadas? ',3),(37,10,'¿Cuál de los siguientes audios corresponde a el himno de la URSS? ',4),(38,10,'¿Cuál de los siguientes audios corresponde a un discurso de John F. ',4),(39,10,'¿Cuál de los siguientes audios corresponde al discurso de Martin Luther King jr.?',4),(40,10,'¿Cuál de los siguientes audios corresponde a un discurso de Hitler? ',4),(41,10,'¿Cual de las siguientes imágenes representa la caída del muro de berlin ? ',4),(42,10,'¿Cual de las siguientes imagenes muestra las piramides de giza?',4),(43,10,'¿Cual de los siguiente es Winston Churchill?',4),(44,10,'¿Cual de las siguientes imagenes muestra la gran muralla china? ',4),(45,10,'¿En qué año comenzó la Segunda Guerra Mundial? ',4),(46,10,'¿Quien fue el primer emperador del imperio romado? ',4),(47,10,'¿Quien fue el primer presidente de EUA? ',4),(48,10,'¿Quién fue el líder de Alemania durante gran parte de la Segunda',4),(49,10,'¿Cuál es el himno nacional de Francia? ',5),(50,10,'El Murciélago Trompudo es una especie endémica de México con una presencia importante en las zonas áridas potosinas, ¿qué sonido produce?  ',5),(51,10,'Estas en el Sotano de las Golondrinas al amanecer, ¿cómo suenan las golondrinas saliendo? ',5),(52,10,'Te encuentras en un festival bajo los cerezos en flor (Sakura) en la ciudad de Kioto. ¿Cuál de estas opciones es una canción tradicional de Japón? ',5),(53,10,'¿Cuál de las siguientes es la bandera de Colombia?  ',5),(54,10,'¿Cuál de estos países es Chile? ',5),(55,10,'¿Cuál de estos monumentos fue construido en la India por un emperador en memoria de su esposa favorita?',5),(56,10,'¿Cuál de estas imágenes muestra el Gran Cañón, una inmensa formación geológica tallada por el río Colorado en Estados Unidos?',5),(57,10,'¿Cuál es el país con la mayor extensión territorial (superficie) del planeta? ',5),(58,10,'¿Cuál es la capital oficial de Australia? ',5),(59,10,'¿Qué río es conocido por ser el más largo de África y uno de los más largos del mundo, cruzando 11 países antes de desembocar en el Mediterráneo?',5),(60,10,'¿Cual es el estado de mayor longitud de mexico? ',5),(61,10,'¿Qué línea de voz le pertenece al personaje de Dante (Devil May Cry))? ',6),(62,10,'¿Cuál de los siguientes sonidos le pertenece a la acción de comer en Minecraft?',6),(63,10,'¿Cuál de los siguientes audios ocurre al recoger monedas en Super Mario World?',6),(64,10,'¿Cuál de los siguientes es el audio del menú principal de Final Fantasy VII?',6),(65,10,'¿Quién de los siguientes presidentes de Nintendo es Hiroshi 	Yamauchi?',6),(66,10,'¿Cuál de las siguientes imágenes es un gameplay de Don´t starve together?',6),(67,10,'¿Quién creo la saga de Uncharted?',6),(68,10,'¿Cuál de las siguientes imágenes es un gameplay de Shadow of the Colossus?',6),(69,10,'¿Cuál fue la fecha de lanzamiento de la PlayStation 1 en Norte América?',6),(70,10,'¿Cuál es considerado el primer juego de la historia?',6),(71,10,'¿Cuál es la consola más vendida en toda la historia de los videojuegos?',6),(72,10,'¿Cómo se llama el estudio que creo títulos como Metal Gear Solid o Castelvania?',6),(73,10,'¿Cual de los siguientes representa el sonido de un léon?',7),(74,10,'¿Cual de los siguientes audios esta en frances?',7),(75,10,'¿Cual de los siguientes sonidos corresponde a un piano? ',7),(76,10,'¿Cual de los siguientes audios corresponde a un elefante? ',7),(77,10,'¿Cual de las siguientes es un leon? ',7),(78,10,'¿Cual es la bandera de japon?',7),(79,10,'¿Cual imagen es de la estatura de la libertad? ',7),(80,10,'¿Cual es una pitaya? ',7),(81,10,'¿Cual es el planeta mas grande del sistema solar?',7),(82,10,'¿Quien escribio Don Quijote de la Manca?',7),(83,10,'¿Cual es el oceano mas grande del mundo? ',7),(84,10,'¿Cual es el metal liquido a temperara ambiente? ',7),(85,10,'Iconica frase del episodio V – El imperio contrataca en StarWars',8),(86,10,'¿Cual de los siguientes sonidos pertenece a Harry Potter?',8),(87,10,'¿Cual de los sonidos pertenece al anime Evangelion?',8),(88,10,'¿Cual musica pertece a la saga de godzilla?',8),(89,10,'¿Quién es el hijo mayor del capo de la mafia “Vito Corleone”?',8),(90,10,'¿Cuál es la cervecería que frecuenta el personaje “Homero Simpson” ',8),(91,10,'¿Cual de las siguientes es la estrella de la muerte? ',8),(92,10,'¿Cual de los siguientes es wall-e?',8),(93,10,'¿Que dia de la semana da clases shrek? ',8),(94,10,'¿Nombre del protagonista de la serie de hora de aventura?',8),(95,10,'¿Nombre del protragonista de la serie dragon ball? ',8),(96,10,'¿Nombre de la serie con mejor calificacion en IMDB? ',8);
/*!40000 ALTER TABLE `pregunta` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-16 15:33:27
