-- MySQL dump 10.13  Distrib 5.6.24, for Win64 (x86_64)
--
-- Host: localhost    Database: pitchfx
-- ------------------------------------------------------
-- Server version	5.6.26-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `atbats`
--

DROP TABLE IF EXISTS `atbats`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `atbats` (
  `ab_guid` varchar(100) NOT NULL,
  `gid` varchar(100) DEFAULT NULL,
  `game_primarykey` int(11) DEFAULT NULL,
  `num` int(11) DEFAULT NULL,
  `balls` tinyint(4) DEFAULT NULL,
  `strikes` tinyint(4) DEFAULT NULL,
  `outs` tinyint(4) DEFAULT NULL,
  `start_tfs` varchar(45) DEFAULT NULL,
  `start_tfs_zulu` varchar(45) DEFAULT NULL,
  `batter` int(11) DEFAULT NULL,
  `stand` varchar(10) DEFAULT NULL,
  `pitcher` int(11) DEFAULT NULL,
  `b_height` varchar(45) DEFAULT NULL,
  `p_throws` varchar(10) DEFAULT NULL,
  `des` text,
  `event_num` int(11) DEFAULT NULL,
  `event` varchar(100) DEFAULT NULL,
  `home_team_runs` int(11) DEFAULT NULL,
  `away_team_runs` int(11) DEFAULT NULL,
  `inning` tinyint(4) DEFAULT NULL,
  `inning_type` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`ab_guid`),
  KEY `idx_1` (`ab_guid`,`game_primarykey`,`pitcher`,`p_throws`,`event`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='This is the table with at bats';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `games`
--

DROP TABLE IF EXISTS `games`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `games` (
  `game_primarykey` int(11) NOT NULL,
  `gid` varchar(100) DEFAULT NULL,
  `home_team` varchar(45) DEFAULT NULL,
  `away_team` varchar(45) DEFAULT NULL,
  `game_date` datetime DEFAULT NULL,
  `home_code` varchar(45) DEFAULT NULL,
  `away_code` varchar(45) DEFAULT NULL,
  `home_abbrev` varchar(45) DEFAULT NULL,
  `away_abbrev` varchar(45) DEFAULT NULL,
  `home_id` varchar(45) DEFAULT NULL,
  `away_id` varchar(45) DEFAULT NULL,
  `total_atbats` int(11) DEFAULT NULL,
  `total_pitches` int(11) DEFAULT NULL,
  PRIMARY KEY (`game_primarykey`),
  KEY `idx_1` (`game_primarykey`,`gid`,`game_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Head table that links everything';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pitches`
--

DROP TABLE IF EXISTS `pitches`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `pitches` (
  `pitch_guid` varchar(100) NOT NULL,
  `ab_guid` varchar(100) DEFAULT NULL,
  `gid` varchar(100) DEFAULT NULL,
  `game_primarykey` int(11) DEFAULT NULL,
  `des` varchar(100) DEFAULT NULL,
  `id` int(11) DEFAULT NULL,
  `type` varchar(10) DEFAULT NULL,
  `tfs` varchar(45) DEFAULT NULL,
  `tfs_zulu` varchar(45) DEFAULT NULL,
  `x` double DEFAULT NULL,
  `y` double DEFAULT NULL,
  `event_num` int(11) DEFAULT NULL,
  `sv_id` varchar(100) DEFAULT NULL,
  `start_speed` double DEFAULT NULL,
  `end_speed` double DEFAULT NULL,
  `sz_top` double DEFAULT NULL,
  `sz_bot` double DEFAULT NULL,
  `pfx_x` double DEFAULT NULL,
  `pfx_z` double DEFAULT NULL,
  `px` double DEFAULT NULL,
  `pz` double DEFAULT NULL,
  `x0` double DEFAULT NULL,
  `y0` double DEFAULT NULL,
  `z0` double DEFAULT NULL,
  `vx0` double DEFAULT NULL,
  `vy0` double DEFAULT NULL,
  `vz0` double DEFAULT NULL,
  `ax` double DEFAULT NULL,
  `ay` double DEFAULT NULL,
  `az` double DEFAULT NULL,
  `break_y` double DEFAULT NULL,
  `break_length` double DEFAULT NULL,
  `break_angle` double DEFAULT NULL,
  `pitch_type` varchar(45) DEFAULT NULL,
  `type_confidence` double DEFAULT NULL,
  `zone` double DEFAULT NULL,
  `nasty` double DEFAULT NULL,
  `spin_dir` double DEFAULT NULL,
  `spin_rate` double DEFAULT NULL,
  `pitcher` int(11) DEFAULT NULL,
  PRIMARY KEY (`pitch_guid`),
  KEY `idx_1` (`game_primarykey`,`ab_guid`,`pitch_guid`,`pitch_type`,`pfx_x`,`gid`,`pitcher`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `players`
--

DROP TABLE IF EXISTS `players`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `players` (
  `id` int(11) NOT NULL,
  `type` varchar(10) DEFAULT NULL,
  `first_name` varchar(80) DEFAULT NULL,
  `last_name` varchar(80) DEFAULT NULL,
  `full_name` varchar(160) DEFAULT NULL,
  `pos` varchar(10) DEFAULT NULL,
  `bats` varchar(10) DEFAULT NULL,
  `throws` varchar(10) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `runners`
--

DROP TABLE IF EXISTS `runners`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `runners` (
  `runner_guid` varchar(100) NOT NULL,
  `ab_guid` varchar(100) NOT NULL,
  `game_primarykey` varchar(100) DEFAULT NULL,
  `id` int(11) DEFAULT NULL,
  `start` varchar(45) DEFAULT NULL,
  `end` varchar(45) DEFAULT NULL,
  `event` varchar(45) DEFAULT NULL,
  `event_num` int(11) DEFAULT NULL,
  `score` varchar(45) DEFAULT NULL,
  `rbi` varchar(45) DEFAULT NULL,
  `earned` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`runner_guid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2016-10-08 21:34:51
