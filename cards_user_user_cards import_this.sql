-- Create users table
CREATE TABLE `users` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `username` VARCHAR(50) NOT NULL,
  `password` VARCHAR(255) NOT NULL,
  PRIMARY KEY (`id`)
);

-- Create cards table
CREATE TABLE `cards` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `passcode` VARCHAR(50) NOT NULL,
  `name` VARCHAR(100) NOT NULL,
  `type` VARCHAR(50) NOT NULL,
  `description` TEXT,
  `atk` INT,
  `def` INT,
  `level` INT,
  `race` VARCHAR(50),
  `attribute` VARCHAR(50),
  `archetype` VARCHAR(50),
  `image_url` VARCHAR(255),
  `image_url_small` VARCHAR(255),
  PRIMARY KEY (`id`)
);

-- Create user_cards table
CREATE TABLE `user_cards` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `passcode` VARCHAR(50) NOT NULL,
  `quantity` INT NOT NULL,
  `name` VARCHAR(100) NOT NULL,
  `type` VARCHAR(50) NOT NULL,
  `description` TEXT,
  `atk` INT,
  `def` INT,
  `level` INT,
  `race` VARCHAR(50),
  `attribute` VARCHAR(50),
  `archetype` VARCHAR(50),
  `card_images` VARCHAR(255),
  PRIMARY KEY (`id`),
  FOREIGN KEY (`user_id`) REFERENCES `users`(`id`)
);
