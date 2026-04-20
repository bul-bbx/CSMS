# CSMS Documentation

## 1. Project Overview
This document provides a thorough overview of the CSMS project, including its functionality, structure, and design elements.

## 2. Architecture and Design Patterns
The CSMS project utilizes various design patterns including MVC, Singleton, and Dependency Injection. Each component is designed to adhere to these principles for modularity and scalability.

## 3. Data Models with Relationships
### User Model
- Attributes: id, username, password, role  
- Relationships: 
    - One-to-Many with Orders  

### Order Model
- Attributes: id, user_id, status, total_price  
- Relationships: 
    - Belongs to User

### Diagrams  
```
User ----< Order
```

## 4. All Controllers and Their Functions
### UserController
- `createUser()`: Creates a new user.  
- `login()`: Handles user authentication.  

### OrderController
- `createOrder()`: Creates a new order.  
- `getOrder()`: Retrieves order details.  

## 5. Services and Business Logic
The CSMS services handle the core business logic, including user management and order processing. This is where crucial validations and data manipulations occur.

## 6. Database Structure
The CSMS database includes tables for users, orders, products, and transactions, designed to facilitate efficient data retrieval and management.

## 7. Authentication and Authorization
CSMS implements JWT for secure authentication, ensuring that users are properly authorized to access resources.

## 8. API Endpoints and Workflows
- `POST /api/users`: Create a new user  
- `POST /api/orders`: Create a new order  

Detailed workflows associated with these endpoints are illustrated below.  
```
User Creation Workflow:
1. User submits signup form
2. System validates data
3. Create user in DB
4. Respond with success
```

# Conclusion
This documentation will continually be updated to reflect changes to the CSMS project. For any questions or contributions, please contact the project maintainers.