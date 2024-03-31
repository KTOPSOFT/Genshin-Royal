CREATE TABLE `bl_game_clans` (
  `id` int(7) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `name` varchar(16) CHARACTER SET latin1 COLLATE latin1_general_cs NOT NULL,
  `tag` varchar(12) NOT NULL,
  `description` varchar(250) NOT NULL,
  `members` varchar(400) NOT NULL,
  `score` int(9) NOT NULL DEFAULT '0',
  `requests` varchar(100) DEFAULT NULL,
  `settings` varchar(11) NOT NULL DEFAULT '0,0,',
  `mcount` int(3) NOT NULL DEFAULT '0',
  `date` varchar(25) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;