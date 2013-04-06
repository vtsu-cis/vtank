#include <master.hpp>
#include <gamesimulation.hpp>

namespace {
	bool instance_running = false;
};

Game_Simulation::Game_Simulation()
{
	if (instance_running) {
		throw new std::runtime_error(
			"Cannot instantiate a second Game_Simulation object.");
	}

	instance_running = true;
}

Game_Simulation::~Game_Simulation()
{
	instance_running = false;
}

void Game_Simulation::update()
{
	
}

int Game_Simulation::generate_id() const
{
	const int arbitrary_cap = 20;
	const int size = static_cast<int>(objects.size()) + arbitrary_cap;
	for (int id = 0; id < size; ++id) {
		if (objects.find(id) == objects.end()) {
			return id;
		}
	}

	VTANK_ASSERT(false); // It should never reach here.
	return -1;
}

int Game_Simulation::add(const game_object &object)
{
	const int new_id = generate_id();
	object->set_id(new_id);
	objects[new_id] = object;

	return new_id;
}

Damageable_Object *Game_Simulation::get(const int id) const
{
	std::map<int, game_object>::const_iterator i = objects.find(id);
	if (i->second->get_id() == id) {
		return &(*i->second);
	}

	return NULL;
}

bool Game_Simulation::remove(const int id)
{
	if (objects.find(id) == objects.end()) {
		return false;
	}
	
	objects.erase(id);
	return true;
}

bool Game_Simulation::remove(const game_object &object)
{
	std::map<int, game_object>::iterator i = objects.begin();
	for (; i != objects.end(); ++i) {
		if (i->second == object) {
			break;
		}
	}
	
	if (i != objects.end()) {
		objects.erase(i);
		return true;
	}
	
	return false;
}
