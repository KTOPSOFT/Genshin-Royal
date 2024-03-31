<?php
include_once ("bl_Common.php");

class Functions
{
    private $conn;

    public function __construct($connection)
    {
        $this->conn = $connection;
    }

    /*
     * OBSOLETE! Update a single column data in the bl_game_users table for the given player account.
     * same function of sql UPDATE query
     *
     * @param field {string} the name of the column that will be update
     * @param value {int/string} the value to update the column with
     * @param where {string} the database table column to filter the records from the bl_game_users table same as the default WHERE clause in sql
     * @param identifier {object} the condition of the sql query, can be anything you want to match the where capsule with e.g id, name, nick, ip, etc...
     * @return {boolean} true if the update query success, false if fail or the update value is the same than was before
    */
    public function update_user_row($field, $value, $where, $identifier)
    {
        $sql = "UPDATE " . PLAYERS_DB . " SET " . $field . "='" . $value . "' WHERE " . $where . "='" . $identifier . "'";
        $this->Query($sql);

        return mysqli_affected_rows($this->conn) > 0;
    }

    /*
     * Update a single column data in the bl_game_users table for the given player account.
     * same function of sql UPDATE query
     *
     * @param field {string} the name of the column that will be update
     * @param value {int/string} the value to update the column with
     * @param where {string} the database table column to filter the records from the bl_game_users table same as the default WHERE clause in sql
     * @param identifier {object} the condition of the sql query, can be anything you want to match the where capsule with e.g id, name, nick, ip, etc...
     * @return {boolean} true if the update query success, false if fail
    */
    public function update_user_row_safe($field, $value, $where, $identifier)
    {
        // manual input validation
        if ($this->validate_sql_parameter($field) == false)
        {
            echo "Invalid parameters provided.";
            return false;
        }

        $sql = "UPDATE " . PLAYERS_DB . " SET " . $field . "=? WHERE " . $where . "=?";
        if ($stmt = $this
            ->conn
            ->prepare($sql))
        {
            $stmt->bind_param('ss', $value, $identifier);
            if ($stmt->execute())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            die(mysqli_error($this->conn));
        }
    }

    /*
     * OBSOLETE! Update multiple columns in the bl_game_users table for the given player account.
     *
     * @param fields {array} the columns names to be updated
     * @param values {array} the values to update the columns with, it have to match with the {fields} array in order and length
     * @param where {string} the database table column to filter the records from the bl_game_users table same as the default WHERE clause in sql
     * @param identifier {object} the condition of the sql query, can be anything you want to match the where capsule with e.g id, name, nick, ip, etc...
     * @return {boolean} true if the update query success, false if fail or the update values are the same than were before
    */
    public function update_user_data($fields, $values, $where, $identifier)
    {
        $inserts = $this->string_to_assoc_array($fields, $values);
        if (!is_array($inserts))
        {
            die("Invalid format, couldn't parse fiels and values.");
        }

        $sql = "UPDATE " . PLAYERS_DB . " SET ";
        foreach (array_keys($inserts) as $key)
        {
            $sql .= $key . "='" . $inserts[$key] . "', ";
        }
        $sql = rtrim($sql, ", ");
        $sql .= " WHERE " . $where . "='" . $identifier . "'";
        $this->Query($sql);

        return mysqli_affected_rows($this->conn) > 0;
    }

    /*
     * Update multiple columns in the bl_game_users table for the given player account.
     *
     * @param fields {array} the columns names to be updated
     * @param values {array} the values to update the columns with, it have to match with the {fields} array in order and length
     * @param where {string} the database table column to filter the records from the bl_game_users table same as the default WHERE clause in sql
     * @param identifier {object} the condition of the sql query, can be anything you want to match the where capsule with e.g id, name, nick, ip, etc...
     * @return {boolean} true if the update query success, false if fail or the update values are the same than were before
    */
    public function update_user_data_safe($fields, $values, $where, $identifier)
    {
        $inserts = $this->string_to_assoc_array($fields, $values);
        if (!is_array($inserts))
        {
            die("Invalid format, couldn't parse fiels and values.");
        }

        $sql = "UPDATE " . PLAYERS_DB . " SET ";
        foreach (array_keys($inserts) as $key)
        {
            if ($this->validate_sql_parameter($key) == false)
            {
                echo "Invalid parameters provided.";
                return false;
            }

            $sql .= $key . "=?, ";
        }
        $sql = rtrim($sql, ", ");

        if ($this->validate_sql_parameter($where) == false)
        {
            echo "Invalid parameters provided.";
            return false;
        }

        $sql .= " WHERE " . $where . "=?";

        if ($stmt = $this
            ->conn
            ->prepare($sql))
        {
            $param_types = '';
            $param_values = [];
            foreach (array_keys($inserts) as $key)
            {
                $param_types .= "s";
                $param_values[] = & $inserts[$key];
            }

            $param_types .= "s";
            $param_values[] = & $identifier;

            array_unshift($param_values, $param_types);
            call_user_func_array([$stmt, 'bind_param'], $param_values);
            if ($stmt->execute())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            die(mysqli_error($this->conn));
        }
    }

    /*
     * Get an user data from the bl_game_users table by the given identifier.
     * This returns all the bl_game_users table columns of the player in case this is found.
     *
     * @param field {object} can by any of the user table (bl_game_users) columns name e.g id, name, nick, ip, etc...
     * @param value {string} the value to search for in the query (same function of WHERE in sql)
     * @return {array} all rows from bl_game_users table of the query result or false if not match have been found
    */
    public function get_user_by($field, $value)
    {
        if ($this->validate_sql_parameter($field) == false)
        {
            echo "Invalid parameters provided.";
            return false;
        }

        $sql = "SELECT * FROM " . PLAYERS_DB . " WHERE " . $field . "=?";
        if ($stmt = $this->conn->prepare($sql))
        {
            $stmt->bind_param('s', $value);
            if ($stmt->execute())
            {
                $stmt->store_result();
                if ($stmt->num_rows <= 0)
                {
                    return false;
                }
                return $this->fetchAssocStatement($stmt);
            }
            else
            {
                echo mysqli_error($this->conn);
                return false;
            }
        }
        else
        {
            echo mysqli_error($this->conn);
            return false;
        }
    }

    /*
     * Get a single row from the user (bl_game_users table)
     * Similar to {get_user_by(...)} with the difference that this return only the specified column
     *
     * @param row_name {string} the name of the column from where the data will be fetch;
     * @param user_id {int} the player account ID = the bl_game_users id column value of the player account
     * @return {object} the expected column data (can be a string or int) or error if fails
    */
    public function get_user_row($user_id, $row_name)
    {
        $result = $this->get_user_by('id', $user_id);
        if($result === false)
        {
          return '';
        }
        return $result[$row_name];
    }

    /*
     * check if the account with the given id exist in the database
     *
     * @param user_id {int} the player database id from the bl_game_users table
     * @return {boolean} true if the an the user account with the given id exist in the database, false otherwise
    */
    public function user_exist($user_id)
    {
        $sql = "SELECT * FROM " . PLAYERS_DB . " WHERE id=?";
        if ($stmt = $this->conn->prepare($sql))
        {
            $stmt->bind_param('i', $user_id);
            if ($stmt->execute())
            {
                $stmt->store_result();
                return $stmt->num_rows > 0;
            }
            else  return false;
        }
        else
        {
            echo mysqli_error($this->conn);
            return false;
        }
    }

    /*
     * check if the account with the given conditional and id exist in the database
     * similar to {UserExist} with the difference that in this function you can identify the account with
     * other property than the id e.g with the nickname of the account
     *
     * @param where {string} the database table column to filter the records from the bl_game_users table same as the default WHERE clause in sql
     * @param index {object} the condition of the sql query, can't be anything you want to match the where capsule with
     * @return {boolean} true if the an the user account with the given identifier exist in the database, false otherwise
    */
    public function user_exist_custom($where, $index)
    {
      if ($this->validate_sql_parameter($where) == false)
        {
            echo "Invalid parameters provided.";
            return false;
        }

        $sql = "SELECT * FROM " . PLAYERS_DB . " WHERE $where=?";
        if ($stmt = $this->conn->prepare($sql))
        {
            $stmt->bind_param('s', $index);
            if ($stmt->execute())
            {
                $stmt->store_result();
                return $stmt->num_rows > 0;
            }
            else  return false;
        }
        else
        {
            echo mysqli_error($this->conn);
            return false;
        }
    }

    /*
     * OBSOLTE, use query_safe(...) instead.
     * Execute a mysqli query and exit on error.
     *
    */
    public function Query($query)
    {
        $result = mysqli_query($this->conn, $query);
        if (mysqli_error($this->conn))
        {
            die(mysqli_error($this->conn));
        }
        return $result;
    }

    /*
     *
     * Execute multiple mysqli query and exit on error.
     *
    */
    public function multiple_query($query)
    {
        $finalResult = mysqli_multi_query($this->conn, $query);
        if ($finalResult)
        {
            do
            {
                if ($result = mysqli_store_result($this->conn) && mysqli_error($this->conn) == '')
                {
                    mysqli_free_result($result);
                }
                else
                {

                }
            }
            while (mysqli_more_results($this->conn) && mysqli_next_result($this->conn));
        }
        else
        {
            die('Multi Query Fail: ' . mysqli_error($this->conn));
        }
        return $finalResult;
    }

    /*
    * Execute a mysqli query and exit on error.
    *
    */
    function query_safe($sql, $sql_types, $parameters)
    {
        if ($stmt = $this
            ->conn
            ->prepare($sql))
        {
            array_unshift($parameters, $sql_types);
            call_user_func_array([$stmt, 'bind_param'], $parameters);
            if ($stmt->execute())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            echo mysqli_error($this->conn);
            return false;
        }
    }

    /*
     * Convert a string formatted queries into an associative array (| is the separator)
     * e.g an http request query could be 'nick|score|coins' for the keys and 'MasterPlayer|100|5' for the values
     * if we want pass these in this function we will get an associative array with the keys as the array keys and values as the array values e.g:
     * [{'nick' => MasterPlayer, 'score' => 100, 'coin' => 5]
     *
     * @param keys {string} the string formatted array, e.g: 'nick|score|coins'
     * @param values {string} the string formatted values, e.g: 'MasterPlayer|100|5'
     * @return {array} the associative array with the fields as keys and values as the values.
    */
    public function string_to_assoc_array($fields, $values)
    {
        $array_fields = $fields;
        $array_values = $values;
        if (!is_array($fields))
        {
            $array_fields = explode('|', $fields);
            $array_values = explode('|', $values);
        }

        return array_combine($array_fields, $array_values);
    }

    /*
     * Add or Deduct coins to an account
     *
     * @param coins {int} the number of coins to add or deduct to the account.
     * @param coinID {int} the MFPS virtual coin id, by default 0 = for the XP coin, 1 = Gold coin
     * @param user_id {int} The player account id, id = the bl_game_users > id column
     * @param op {int} type of operation, 1 for add coins to the existing coins of the player account, 0 to deduct
     * @return {string} returns the new coins total formatted e.g 1000&500 = 1000 XP coins and 500 Gold coins, & = the separator.
    */
    public function insert_coins($coins, $coinID, $user_id, $op = 1)
    {
        $current_coins_row = $this->get_user_row($user_id, 'coins');
        $split_coins = explode('&', $current_coins_row);
        $current_coin = (int)($split_coins[(int)$coinID]);

        if ($op == 1)
        {
            $current_coin = $current_coin + $coins;
        }
        else
        {
            $current_coin = $current_coin - $coins;
        }

        $split_coins[(int)$coinID] = $current_coin;
        $current_coins_row = implode('&', $split_coins);
        return $current_coins_row;
    }

    /**
     * Validate string as a sql parameter without special sql characters.
     *
     *
     */
    public function validate_sql_parameter($sql)
    {
        if (strpos($sql, '=') !== false || strpos($sql, ',') !== false)
        {
            return false;
        }
        return true;
    }

    /**
     * Fetch parameters from prepared statement.
     *
     *
     */
    function fetchAssocStatement($stmt)
    {
        if($stmt->num_rows>0)
        {
        $result = array();
        $md = $stmt->result_metadata();
        $params = array();
        while($field = $md->fetch_field()) {
            $params[] = &$result[$field->name];
        }
        call_user_func_array(array($stmt, 'bind_result'), $params);
        if($stmt->fetch())
            return $result;
        }

    return null;
    }
}
?>