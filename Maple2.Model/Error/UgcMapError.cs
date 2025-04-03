// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum UgcMapError : byte {
    [Description("")]
    s_ugcmap_ok = 0,
    [Description("")]
    s_empty_string = 1,
    [Description("There are still placed items.")]
    s_ugcmap_create_on_non_empty_area = 2,
    [Description("System Error: Item does not exist.")]
    s_ugcmap_not_exist_craft_item = 3,
    [Description("You do not own this item.")]
    s_ugcmap_not_owned_item = 4,
    [Description("That cannot be placed here.")]
    s_ugcmap_cant_be_created = 5,
    [Description("This cannot be placed at this location.")]
    s_ugcmap_cant_create_on_place = 6,
    [Description("That cannot be placed at this location.")]
    s_ugcmap_no_base_cube = 7,
    [Description("You do not own this area.")]
    s_ugcmap_dont_have_ownership = 8,
    [Description("This terrain cannot be placed above other terrain.")]
    s_ugcmap_cant_create_ground_on_ground = 9,
    [Description("Can't put this above terrain.")]
    s_ugcmap_cant_create_on_ground = 10,
    [Description("This can only be placed above terrain.")]
    s_ugcmap_only_be_created_on_ground = 11,
    [Description("That cannot be placed here.")]
    s_ugcmap_cant_stack_on = 12,
    [Description("System Error")]
    s_ugcmap_db = 13,
    [Description("System Error")]
    s_ugcmap_center = 14,
    [Description("You must have a nearby wall to place a wall decoration")]
    s_ugcmap_no_wall_to_attach = 15,
    [Description("This item cannot be placed on the wall.")]
    s_ugcmap_not_wall_attachable = 16,
    [Description("Wall decorations can only be placed on terrain block walls.")]
    s_ugcmap_only_be_created_on_wall = 17,
    [Description("I can't put that on this wall.")]
    s_ugcmap_cant_attached_to_this_wall = 18,
    [Description("That item has already been placed.")]
    s_ugcmap_have_already_attached = 19,
    [Description("There are no items to collect.")]
    s_ugcmap_no_cube_to_remove = 20,
    [Description("That cannot be retrieved.")]
    s_ugcmap_cant_be_removed = 21,
    [Description("The stacked items must be collected first.")]
    s_ugcmap_cant_remove_before_remove_all_stacked = 22,
    [Description("")]
    s_ugcmap_cant_remove_building_with_indoor_items = 23,
    [Description("")]
    s_ugcmap_can_be_remove_from_wall = 24,
    [Description("There is no wall decoration in the direction you are facing.")]
    s_ugcmap_no_attached_object = 25,
    [Description("There are no items to rotate.")]
    s_ugcmap_no_cube_to_rotate = 26,
    [Description("The default terrain cannot be rotated.")]
    s_ugcmap_cant_rotate_default_cube = 27,
    [Description("There are no items to exchange.")]
    s_ugcmap_no_cube_to_replace = 28,
    [Description("You cannot replace this with a furnishing of a different type.")]
    s_ugcmap_cant_be_replaced = 29,
    [Description("A different wall decoration has been placed.")]
    s_ugcmap_attached_cube_exist = 30,
    [Description("This can only be exchanged for items that can be stacked.")]
    s_ugcmap_cant_replace_stackable_with_not_stackable = 31,
    [Description("This location cannot be bought.")]
    s_ugcmap_not_a_buyable = 32,
    [Description("You do not have enough funds to complete the purchase.")]
    s_ugcmap_not_enough_money = 33,
    [Description("Another player has already completed the purchase.")]
    s_ugcmap_already_owned = 35,
    [Description("The contract cannot be canceled.")]
    s_ugcmap_salable = 36,
    [Description("There are no objects to lift.")]
    s_ugcmap_no_cube_to_lift = 37,
    [Description("This object cannot be lifted.")]
    s_ugcmap_cant_lift_ugc_cube = 38,
    [Description("You can't pick up items that belong to someone else.")]
    s_ugcmap_cant_lift_salable = 39,
    [Description("The default terrain cannot be retrieved.")]
    s_ugcmap_cant_remove_default_cube = 40,
    [Description("Collect the attached items first.")]
    s_ugcmap_cant_remove_cube_with_attached = 41,
    [Description("System Error: Furnishing information not found.")]
    s_ugcmap_null_cube_item_info = 42,
    [Description("You cannot place that above the home's ceiling.")]
    s_ugcmap_height_limit = 43,
    [Description("You can't place that here.")]
    s_ugcmap_area_limit = 44,
    [Description("You cannot place any more of this structure.")]
    s_ugcmap_building_count = 45,
    [Description("You cannot purchase at this time.")]
    s_ugcmap_not_for_sale = 46,
    [Description("")]
    s_ugcmap_no_more_room = 47,
    [Description("You have not visited the house. Click the house button to go there.")]
    s_ugcmap_no_home = 48,
    [Description("You have already purchased this home expansion.")]
    s_ugcmap_my_house = 49,
    [Description("The contract has expired.")]
    s_ugcmap_already_expired = 50,
    [Description("There is an item placed at the house.")]
    s_ugcmap_have_equipitems = 52,
    [Description("That item is already placed.")]
    s_ugcmap_cant_replace_same_cube = 53,
    [Description("You cannot buy this because you have already bought another area.")]
    s_ugcmap_cant_buy_more_than_two_house = 54,
    [Description("You do not meet the trophy requirements for this home expansion.")]
    s_ugcmap_need_trophy = 55,
    [Description("There are no items to place.")]
    s_ugcmap_try_place_empty = 57,
    [Description("This item cannot be exchanged.")]
    s_ugcmap_cant_replace_type = 58,
    [Description("This wall decoration cannot be rotated.")]
    s_ugcmap_cant_rotate_attached = 59,
    [Description("You cannot enter Furnishing Mode here.")]
    s_ugcmap_cant_guide_build = 60,
    [Description("This cannot be placed below a wall decoration.")]
    s_ugcmap_cant_create_under_attach = 61,
    [Description("A wall decoration cannot be placed above this item.")]
    s_ugcmap_cant_attach_upper_cube = 62,
    [Description("This cannot be placed below a wall decoration.")]
    s_ugcmap_cant_replace_under_attach = 63,
    [Description("You have already placed the maximum number of servants.")]
    s_ugcmap_cant_place_maid = 64,
    [Description("This can only be placed on the ground.")]
    s_ugcmap_only_place_on_the_floor = 65,
    [Description("The duration cannot be extended yet.")]
    s_ugcmap_not_extension_date = 66,
    [Description("You do not have the necessary funds to extend the contract.")]
    s_ugcmap_need_extension_pay = 67,
    [Description("This area is waiting to be sold.")]
    s_ugcmap_expired_salable_group = 68,
    [Description("Please sell while outdoors.")]
    s_ugcmap_cant_sell_my_home_in_indoor = 69,
    [Description("Houses planned for redevelopment cannot be purchased.")]
    s_ugcmap_blocked_salable_group = 70,
    [Description("System Error: Please try again later")]
    s_ugcmap_retry_later = 71,
    [Description("The block is being exchanged. Please try again later.")]
    s_ugcmap_waiting_for_cube_to_be_replaced = 72,
    [Description("The block is being placed. Please try again later.")]
    s_ugcmap_waiting_for_cube_to_be_created = 73,
    [Description("The block is being removed. Please try again later.")]
    s_ugcmap_waiting_for_cube_to_be_removed = 74,
    [Description("This cannot be placed.")]
    s_ugcmap_trigger_count = 75,
    [Description("This house cannot be recommended.")]
    s_ugcmap_no_owner_to_commend = 76,
    [Description("Star Architect nomination failed. Please try again later.")]
    s_ugcmap_add_commend_home_fail_from_db = 77,
    [Description("You cannot nominate yourself.")]
    s_ugcmap_cant_commend_myself = 78,
    [Description("Contains a forbidden word.")]
    s_ugcmap_ban_word_included = 79,
    [Description("This is not your house.")]
    s_ugcmap_not_my_house = 80,
    [Description("Already nominated.")]
    s_ugcmap_cant_commend_duplicate = 81,
    [Description("This cannot be expanded any further.")]
    s_ugcmap_cant_extend_area_level_anymore = 83,
    [Description("No more bonuses can be collected today.")]
    s_ugcmap_cant_take_interior_gift_more = 84,
    [Description("Bonuses cannot be received at the moment. Please try again later.")]
    s_ugcmap_take_interior_gift_fail_from_db = 85,
    [Description("You have already received the bonus.")]
    s_ugcmap_already_taken_interior_grade_gift = 86,
    [Description("This cannot be expanded any further.")]
    s_ugcmap_cant_extend_height_level_anymore = 87,
    [Description("You don't meet the requirements to place this.")]
    s_ugcmap_cube_lock = 88,
    [Description("You cannot buy that furnishing.")]
    s_ugcmap_cant_additionalbuy = 89,
    [Description("You cannot use this while furnishing.")]
    s_err_cannot_use_in_design_home = 90,
    [Description("You cannot place furnishings that are not currently available for purchase.")]
    s_err_cannot_buy_limited_item_more = 91,
    [Description("That furnishing cannot be placed in the current mode.")]
    s_err_cannot_install_blueprint = 92,
    [Description("Assistants cannot be placed in the current mode.")]
    s_err_cannot_install_maid_in_practice = 93,
    [Description("GMs only.")]
    s_ugcmap_admin_only = 94,
    [Description("This item cannot be used.")]
    s_ugcmap_not_allowed_item = 95,
    [Description("You do not have enough mesos.")]
    s_err_ugcmap_not_enough_meso_balance = 96,
    [Description("You do not have enough merets.")]
    s_err_ugcmap_not_enough_merat_balance = 97,
    [Description("Double-click the designer item icon to edit the template again.")]
    s_err_ugcmap_cant_build_empty_ugc = 98,
    [Description("This cannot be placed.")]
    s_ugcmap_installnpc_count = 99,
    [Description("You can't earn any more experience from decorating today.")]
    s_err_ugcmap_construct_exp_overtime = 100,
    [Description("Life Skill items cannot be placed in the current mode.")]
    s_err_cannot_install_nurturing_in_design_home = 101,
    [Description("That furnishing cannot be placed in the current mode.")]
    s_ugcmap_not_use_blueprint_item = 102,
    [Description("Interior portals cannot be placed in the current mode.")]
    s_err_cannot_install_magic_portal = 103,
    [Description("Trigger editors cannot be placed in the current mode.")]
    s_err_cannot_install_trigger_editor = 104,
    [Description("Trigger control items cannot be placed in the current mode.")]
    s_err_cannot_install_trigger_controlobject = 105,
    [Description("Interior message items cannot be placed in the current mode.")]
    s_err_cannot_install_interior_message = 106,
    [Description("Event items cannot be placed in the current mode.")]
    s_err_cannot_install_event_cube = 107,
    [Description("Trophy-related furnishings cannot be placed in the current mode.")]
    s_err_cannot_install_trophy_relative_cube = 108,
    [Description("Assistant cooking/alchemy/jeweling stations cannot be placed in the current mode.")]
    s_err_cannot_install_workbench_cube = 109,
    [Description("Mannequins cannot be placed in the current mode.")]
    s_err_cannot_install_fittingdoll = 110,
    [Description("UGC items made by others cannot be placed in the current mode.")]
    s_err_cannot_install_ugcdesign_maidin_other = 111,

    [Description("Due to a system error, the contract cannot be made.")]
    s_ugcmap_system_error = byte.MaxValue,
}
