/*!
    \file   utility.hpp
    \brief  Declares functions in the Utility namespace.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef UTILITY_HPP
#define UTILITY_HPP

namespace Utility {
	/*!
		The Rectangle struct is simply a collection of attributes that belong to a rectangle.
		Rectangles are used in collision detection.
	*/
	struct Rectangle {
		double x;
		double y;
		int width;
		int height;

		Rectangle() : x(0), y(0), width(0), height(0) {}

		Rectangle(const double &x, const double &y, const int &width, const int &height)
		{
			this->x = x;
			this->y = y;
			this->width = width;
			this->height = height;
		}
	};

	struct Line {
		double x1, y1, x2, y2;

		Line() : x1(0), y1(0), x2(1), y2(1) {}
		Line(const double &x1, const double &y1, const double &x2, const double &y2)
		{
			this->x1 = x1;
			this->y1 = y1;
			this->x2 = x2;
			this->y2 = y2;
		}

		/*!
			\return -1 if the two lines are parallel, 1 if they intersect,
			0 if they don't.
		*/
		int intersects(const Line &other)
		{
			const double d = (x2 - x1) * (other.y2 - other.y1) - (y2 - y1) * (other.x2 - other.x1);
			if (abs(d) < 0.001)
				return -1;
			
			const double AB = ((y1 - other.y1) * (other.x2 - other.x1) -
				(x1 - other.x1) * (other.y2 - other.y1)) / d;

			if (AB > 0.0 && AB < 1.0) {
				const double CD = ((y1 - other.y1) * (x2 - x1) - (x1 - other.x1) * (y2 - y1)) / d;
				if (CD > 0.0 && CD < 1.0)
					return 1;
			}

			return 0;
		}

		int intersects(int other_x1, int other_y1, int other_x2, int other_y2)
		{
			return intersects(Line(other_x1, other_y1, other_x2, other_y2));
		}
	};

	static inline bool operator==(const Line &l1, const Line &l2)
	{
		return l1.x1 == l2.x1 && l1.y1 == l2.y1 && l1.x2 == l2.x2 && l1.y2 == l2.y2;
	}

	static inline bool operator!=(const Line &l1, const Line &l2)
	{
		return !(l1 == l2);
	}

	/*!
		Check if a collision exists between a projectile and a wall.
		\param projectile Projectile to test.
		\param current_map Map to test against.
		\return True if a collision exists.
	*/
	bool wall_collision(const projectile_ptr, const Map *);

	/*!
		Check if a collision exists between a line segment and a circle.
		\param circle_x X position of the circle.
		\param circle_y Y position of the circle.
		\param radius Radius of the circle.
		\param x1 Line starting position X.
		\param y1 Line starting position Y.
		\param x2 Line ending position X.
		\param y2 Line ending position Y.
		\return True if a collision exists between the line and circle; false otherwise.
	*/
	bool line_circle_collision(double, double,
		double, double, double, double, double);
	
	/*!
		Check if a collision exists between a player and a wall.
		\param player Player to test.
		\param current_map Map to test against.
		\return True if a collision exists.
	*/
	bool wall_collision(const tank_ptr, const Map *);

	/*!
		Check if a collision exists between a projectile and a player.
		\param projectile Projectile to test.
		\param player Player to test against.
		\return True if a collision exists.
	*/
	bool projectile_collision(const projectile_ptr, const tank_ptr);

	/*!
		Check if a collision exists between a projectile and a circle at some point.
		\param projectile Projectile to test.
		\param position Position of the object to test against.
		\param radius Radius of the circle to test against.
		\return True if a collision exists; false otherwise.
	*/
	bool projectile_collision(const projectile_ptr projectile, 
		const VTankObject::Point &position, float radius);

	/*!
		Check if a collision exists between two players.
		\param p1 First player to test.
		\param p2 Second player to test against.
		\return True if a collision exists.
	*/
	bool player_collision(const tank_ptr, const tank_ptr);
	
	/*!
		Low-level method for checking a collision between a circle at a certain point
		with a certain radius and a rectangle at a certain point.
		\param circle_pos Position of the circle.
		\param radius Radius of the circle.
		\param rect Rectangle parameters.
	*/
	bool circle_to_rectangle_collision(const VTankObject::Point &, const float &,
		const Rectangle &);
	
	/*!
		Test if a circle has collided with another circle.
		\param c1 Position of the first circle.
		\param r1 Radius of the first circle.
		\param c2 Position of the second circle.
		\param r2 Radius of the second circle.
		\return True if a collision exists.
	*/
	bool circle_collision(const VTankObject::Point &, const float &, 
		const VTankObject::Point &, const float &);

    /*!
        Convert a VTankObject::GameMode to it's string equivalent.
        \param mode Mode to convert.
        \return User-friendly string to read.
    */
    std::string to_string(const VTankObject::GameMode);

	/*!
		Convert upper-case letters to lower-case letters.
		\param str String to convert.
		\return Lower-case string.
	*/
	std::string to_lower(std::string str);

    /*!
        Check if a vector contains a value. It uses the '==' operator to compare the values.
        \param source Source vector to search.
        \param value Value to look for.
        \return True if value is contained in source; false otherwise.
    */
    bool contains(const std::vector<int>& source, const int value);

	/*!
		Check if an array contains a value.
		\param source Source array to search.
		\param size Number of elements to look through in 'source'.
		\param value Value to search for.
		\return True if the value is contained in source; false otherwise.
	*/
	bool contains(const int *source, const int size, const int value);
	
	/*!
		Round a double to it's nearest decimal place.
	*/
	int round(double n);

	struct Circle
	{
		float radius;
		VTankObject::Point position;

		Circle() : radius(0), position() {}
		Circle(float c_radius, VTankObject::Point c_center_position)
			: radius(c_radius), position(c_center_position) {}

		bool intersects(const Circle &other) const
		{
			return circle_collision(position, radius, other.position, other.radius);
		}
	};
};

#endif
