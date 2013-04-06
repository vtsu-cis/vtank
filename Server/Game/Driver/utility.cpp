
#include <master.hpp>
#include <player.hpp>
#include <projectilemanager.hpp>
#include <Map.hpp>
#include <utility.hpp>

namespace Utility {
	/*!
		Check if a circle overlaps with a rectangle.
		\param circle_pos Position of the center of the circle.
		\param radius Radius of the circle.
		\param rect Rectangle to test against.
	*/
	bool circle_to_rectangle_collision(
		const VTankObject::Point &circle_pos, const float &radius, const Rectangle &rect)
    {
        const double half_width = rect.width / 2;
        const double half_height = rect.height / 2;

		const double distance_x = abs(circle_pos.x - rect.x - half_width);
        const double distance_y = abs(circle_pos.y - rect.y - half_height);

        // Quick rule-out checks.
        if (distance_x > half_width + radius || distance_y > half_height + radius)
            return false;

        if (distance_x <= half_width || distance_y <= half_height)
            return true;
        
        // Measure the distance between the corner and the circle.
		// TODO: Which one is correct?
        /*return sqrt(pow(circle_distance.x - half_width, 2)) +
            sqrt(pow(circle_distance.y - half_height, 2)) <= radius;*/
		return sqrt(pow(distance_x - half_width, 2) + 
			pow(distance_y - half_height, 2)) <= radius;
    }

	bool circle_collision(const VTankObject::Point &c1, const float &r1, 
		const VTankObject::Point &c2, const float &r2)
	{
		return sqrt(pow(c1.y - c2.y, 2) + pow(c1.x - c2.x, 2)) < (r1 + r2);
	}

	bool wall_collision(const projectile_ptr projectile, const Map *current_map)
	{
		// Determine the range of tiles to check.
        const int tiles_x = static_cast<int>((projectile->position.x / TILE_SIZE) + 2);
        const int tiles_y = static_cast<int>((-projectile->position.y / TILE_SIZE) + 2);

        const int min_x = static_cast<int>((projectile->position.x / TILE_SIZE) - tiles_x);
        const int min_y = static_cast<int>((-projectile->position.y / TILE_SIZE) - tiles_y);
        const int max_x = static_cast<int>((projectile->position.x / TILE_SIZE) + tiles_x);
        const int max_y = static_cast<int>((-projectile->position.y / TILE_SIZE) + tiles_y);

        // Determine the fake "circles" on the tank.
        const double angle = projectile->angle;
		const float radius = projectile->type.projectile.collision_radius;
		VTankObject::Point circle;
        // Calculate the offset for the circle.
        circle.x = projectile->position.x + cos(angle) * radius;
        circle.y = projectile->position.y + sin(angle) * radius;

        // Iterate through tiles.
        for (int y = min_y; y < current_map->get_height() && y <= max_y; y++) {
            if (y < 0)
                continue;

            for (int x = min_x; x < current_map->get_width() && x <= max_x; x++) {
                if (x < 0)
                    continue;

                const Tile tile = current_map->get_tile(x, y);
				if (!tile.passable) {
                    // Do collision check.
					// TODO: Why is an offset necessary here?
                    const Rectangle rect(x * TILE_SIZE, -(y * TILE_SIZE + TILE_SIZE), TILE_SIZE, TILE_SIZE);
					if (circle_to_rectangle_collision(circle, radius, rect))
                        return true;
                }
            }
        }

		return false;
	}

	bool wall_collision(const tank_ptr player, const Map *current_map)
	{
		return false;
	}

	bool line_circle_collision(double circle_x, double circle_y,
		double radius, double x1, double y1, double x2, double y2)
	{
		const double line_vector[2] = {x2 - x1, y2 - y1};
		const double vector_to_line[2] = {x1 - circle_x, y1 - circle_y};
		
		// Quadratic formula: ax^2 + bx + c = 0
		double a, b, c;
		double sqrtterm, res1, res2;
		
		a = pow(line_vector[0], 2) + pow(line_vector[1], 2);
		b = 2 * (vector_to_line[0] * line_vector[0] +
			vector_to_line[1] * line_vector[1]);
		c = (pow(vector_to_line[0], 2) + pow(vector_to_line[1], 2)) - pow(radius, 2);
		
		sqrtterm = pow(b, 2) - 4 * a * c;
		
		if (sqrtterm < 0) {
			return false;
		}
		
		sqrtterm = sqrt(sqrtterm);
		res1 = (-b - sqrtterm) / (2 * a);
		
		if (res1 >= 0 && res1 <= 1) {
			return true;
		}
		
		res2 = (-b + sqrtterm) / (2 * a);
		if (res2 >= 0 && res2 <= 1) {
			return true;
		}
		
		return false;
	}

	bool projectile_collision(const projectile_ptr projectile, const tank_ptr player)
	{
		double angle = projectile->angle;
		const float bullet_radius = projectile->type.projectile.collision_radius;

		VTankObject::Point c1;
        c1.x = projectile->position.x + (cos(angle) * bullet_radius);
        c1.y = projectile->position.y + (sin(angle) * bullet_radius);
		
        angle = player->get_angle();
		const double distance_x = cos(angle) * TANK_SPHERE_RADIUS;
		const double distance_y = sin(angle) * TANK_SPHERE_RADIUS;

        const VTankObject::Point original = player->get_position();

		VTankObject::Point c2(original);
        c2.x += distance_x;
        c2.y += distance_y;

		VTankObject::Point c3(original);
        c3.x -= distance_x;
        c3.y -= distance_y;

		return (circle_collision(c1, bullet_radius, c2, TANK_SPHERE_RADIUS) ||
			circle_collision(c1, bullet_radius, c3, TANK_SPHERE_RADIUS));
	}

	bool projectile_collision(const projectile_ptr projectile, 
		const VTankObject::Point &position, float radius)
	{
		double angle = projectile->angle;
		const float bullet_radius = projectile->type.projectile.collision_radius;

		VTankObject::Point c1;
        c1.x = projectile->position.x + (cos(angle) * bullet_radius);
        c1.y = projectile->position.y + (sin(angle) * bullet_radius);
		
        angle = 0;
		const double distance_x = cos(angle) * radius;
		const double distance_y = sin(angle) * radius;

        const VTankObject::Point original = position;

		VTankObject::Point c2(original);
        c2.x += distance_x;
        c2.y += distance_y;

		VTankObject::Point c3(original);
        c3.x -= distance_x;
        c3.y -= distance_y;

		return (circle_collision(c1, bullet_radius, c2, radius) ||
			circle_collision(c1, bullet_radius, c3, radius));
	}

	bool player_collision(const tank_ptr p1, const tank_ptr p2)
	{
		return false;
	}

    std::string to_string(const VTankObject::GameMode mode)
    {
        switch (mode) {
            case VTankObject::DEATHMATCH:
                return "Deathmatch";
            case VTankObject::TEAMDEATHMATCH:
                return "Team Deathmatch";
            case VTankObject::CAPTURETHEFLAG:
                return "Capture the Flag";
            case VTankObject::CAPTURETHEBASE:
                return "Capture the Base";
        }

        return "Unknown";
    }

	std::string to_lower(std::string str)
	{
		const std::string::size_type size = str.size();
		for (std::string::size_type i = 0; i < size; i++) {
			if (str[i] >= 0x41 && str[i] <= 0x5A) {
				str[i] = str[i] + 0x20;
			}
		}

		return str;
	}
	
    bool contains(const std::vector<int>& source, const int value) 
    {
        for (std::vector<int>::size_type i = 0; i != source.size(); i++) {
            if (source[i] == value) {
                return true;
            }
        }

        return false;
    }

	bool contains(const int *source, const int size, const int value)
	{
		for (int i = 0; i < size; ++i) {
			if (source[i] == value) {
				return true;
			}
		}

		return false;
	}

	int round(double n)
	{
		const double t = n - floor(n);
		if (t >= 0.5) {
			n = ceil(n);
		}
		else {
			n = floor(n);
		}

		return static_cast<int>(n);
	}
};