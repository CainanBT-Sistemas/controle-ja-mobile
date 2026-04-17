package com.cainanbt.controleja.data.remote

import com.cainanbt.controleja.data.model.*
import retrofit2.Response
import retrofit2.http.*

interface ApiService {

    // Auth
    @POST("auth")
    suspend fun login(@Body request: LoginRequest): Response<UserResponse>

    @POST("auth/auto-login")
    suspend fun autoLogin(@Body body: Map<String, String>): Response<UserResponse>

    // Users
    @POST("users/register")
    suspend fun register(@Body user: InsertUpdateUserDTO): Response<UserResponse>

    @PUT("users/change-password")
    suspend fun changePassword(@Body body: Map<String, String>): Response<Unit>

    @PUT("users/profile")
    suspend fun updateProfile(@Body body: Map<String, String>): Response<Unit>

    @DELETE("users/{id}")
    suspend fun deleteAccount(@Path("id") userId: String): Response<Unit>

    @POST("users/reset")
    suspend fun resetData(): Response<Unit>

    // Transactions
    @GET("transactions")
    suspend fun getTransactions(
        @Query("start") start: Long,
        @Query("end") end: Long
    ): Response<List<Transaction>>

    @POST("transactions")
    suspend fun createTransaction(@Body transaction: Transaction): Response<Transaction>

    @PUT("transactions/{id}")
    suspend fun updateTransaction(
        @Path("id") id: String,
        @Body transaction: Transaction,
        @Query("updateFuture") updateFuture: Boolean = false
    ): Response<Transaction>

    @DELETE("transactions/{id}")
    suspend fun deleteTransaction(
        @Path("id") id: String,
        @Query("cancelFuture") cancelFuture: Boolean = false
    ): Response<Unit>

    // Accounts
    @GET("accounts")
    suspend fun getAccounts(): Response<List<Account>>

    @POST("accounts")
    suspend fun createAccount(@Body account: Account): Response<Account>

    @PUT("accounts/{id}")
    suspend fun updateAccount(@Path("id") id: String, @Body account: Account): Response<Account>

    @DELETE("accounts/{id}")
    suspend fun deleteAccount2(@Path("id") id: String): Response<Unit>

    // Categories
    @GET("categories")
    suspend fun getCategories(): Response<List<Category>>

    @POST("categories")
    suspend fun createCategory(@Body category: Category): Response<Category>

    @PUT("categories/{id}")
    suspend fun updateCategory(@Path("id") id: String, @Body category: Category): Response<Category>

    @DELETE("categories/{id}")
    suspend fun deleteCategory(@Path("id") id: String): Response<Unit>

    // Credit Cards
    @GET("credit-cards")
    suspend fun getCreditCards(): Response<List<CreditCard>>

    @POST("credit-cards")
    suspend fun createCreditCard(@Body card: CreditCard): Response<CreditCard>

    @PUT("credit-cards/{id}")
    suspend fun updateCreditCard(@Path("id") id: String, @Body card: CreditCard): Response<CreditCard>

    @DELETE("credit-cards/{id}")
    suspend fun deleteCreditCard(@Path("id") id: String): Response<Unit>

    // Vehicles
    @GET("vehicles")
    suspend fun getVehicles(): Response<List<Vehicle>>

    @POST("vehicles")
    suspend fun createVehicle(@Body vehicle: Vehicle): Response<Vehicle>

    @PUT("vehicles/{id}")
    suspend fun updateVehicle(@Path("id") id: String, @Body vehicle: Vehicle): Response<Vehicle>

    @DELETE("vehicles/{id}")
    suspend fun deleteVehicle(@Path("id") id: String): Response<Unit>

    // Dashboard
    @GET("dashboard/expenses-category")
    suspend fun getExpensesByCategory(
        @Query("start") start: Long,
        @Query("end") end: Long
    ): Response<List<ChartData>>

    @GET("dashboard/credit-expenses-category")
    suspend fun getCreditExpensesByCategory(
        @Query("start") start: Long,
        @Query("end") end: Long
    ): Response<List<ChartData>>

    @GET("dashboard/incomes-category")
    suspend fun getIncomesByCategory(
        @Query("start") start: Long,
        @Query("end") end: Long
    ): Response<List<ChartData>>

    @GET("dashboard/fuel-comparison")
    suspend fun getFuelComparison(
        @Query("start") start: Long,
        @Query("end") end: Long
    ): Response<List<ChartData>>

    @GET("dashboard/summary")
    suspend fun getDashboardSummary(
        @Query("start") start: Long,
        @Query("end") end: Long
    ): Response<FinancialSummary>

    @GET("dashboard/evolution")
    suspend fun getEvolution(
        @Query("start") start: Long,
        @Query("end") end: Long,
        @Query("categoryId") categoryId: String? = null
    ): Response<List<ChartData>>

    @GET("dashboard/full-summary")
    suspend fun getFullSummary(
        @Query("start") start: Long,
        @Query("end") end: Long
    ): Response<DashboardFullSummary>

    // Invoices
    @GET("invoices/{cardId}/{month}/{year}")
    suspend fun getInvoiceDetails(
        @Path("cardId") cardId: String,
        @Path("month") month: Int,
        @Path("year") year: Int
    ): Response<InvoiceDetailsDTO>

    @GET("invoices/{cardId}/{month}/{year}/advanceable")
    suspend fun getAdvanceablePurchases(
        @Path("cardId") cardId: String,
        @Path("month") month: Int,
        @Path("year") year: Int
    ): Response<List<AdvanceablePurchaseDTO>>

    @POST("invoices/{invoiceId}/refund")
    suspend fun processRefund(
        @Path("invoiceId") invoiceId: String,
        @Body dto: RefundRequestDTO
    ): Response<Unit>

    @POST("invoices/{invoiceId}/advance")
    suspend fun advanceInstallments(
        @Path("invoiceId") invoiceId: String,
        @Body dto: AdvanceRequestDTO
    ): Response<Unit>
}
