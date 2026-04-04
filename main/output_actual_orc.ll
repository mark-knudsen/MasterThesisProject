; ModuleID = 'repl_module'
source_filename = "repl_module"

define ptr @main_0() {
entry:
  %csv_boxed_res = call ptr inttoptr (i64 140722553499812 to ptr)(ptr inttoptr (i64 2646027036784 to ptr), ptr inttoptr (i64 2646027144224 to ptr))
  %unbox_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %csv_boxed_res, i32 0, i32 1
  %raw_rows_ptr = load ptr, ptr %unbox_ptr, align 8
  %names_header = call ptr inttoptr (i64 140727031823776 to ptr)(i64 24)
  %names_data = call ptr inttoptr (i64 140727031823776 to ptr)(i64 40)
  %ptr_0 = getelementptr ptr, ptr %names_data, i64 0
  store ptr inttoptr (i64 2646027145024 to ptr), ptr %ptr_0, align 8
  %ptr_1 = getelementptr ptr, ptr %names_data, i64 1
  store ptr inttoptr (i64 2646027144384 to ptr), ptr %ptr_1, align 8
  %ptr_2 = getelementptr ptr, ptr %names_data, i64 2
  store ptr inttoptr (i64 2646027144336 to ptr), ptr %ptr_2, align 8
  %ptr_3 = getelementptr ptr, ptr %names_data, i64 3
  store ptr inttoptr (i64 2646027144400 to ptr), ptr %ptr_3, align 8
  %ptr_4 = getelementptr ptr, ptr %names_data, i64 4
  store ptr inttoptr (i64 2646027144240 to ptr), ptr %ptr_4, align 8
  %len = getelementptr inbounds nuw { i64, i64, ptr }, ptr %names_header, i32 0, i32 0
  store i64 5, ptr %len, align 4
  %cap = getelementptr inbounds nuw { i64, i64, ptr }, ptr %names_header, i32 0, i32 1
  store i64 5, ptr %cap, align 4
  %data = getelementptr inbounds nuw { i64, i64, ptr }, ptr %names_header, i32 0, i32 2
  store ptr %names_data, ptr %data, align 8
  %types_header = call ptr inttoptr (i64 140727031823776 to ptr)(i64 24)
  %types_data = call ptr inttoptr (i64 140727031823776 to ptr)(i64 40)
  %ptr = getelementptr ptr, ptr %types_data, i64 0
  store ptr inttoptr (i64 1 to ptr), ptr %ptr, align 8
  %ptr1 = getelementptr ptr, ptr %types_data, i64 1
  store ptr inttoptr (i64 4 to ptr), ptr %ptr1, align 8
  %ptr2 = getelementptr ptr, ptr %types_data, i64 2
  store ptr inttoptr (i64 1 to ptr), ptr %ptr2, align 8
  %ptr3 = getelementptr ptr, ptr %types_data, i64 3
  store ptr inttoptr (i64 3 to ptr), ptr %ptr3, align 8
  %ptr4 = getelementptr ptr, ptr %types_data, i64 4
  store ptr inttoptr (i64 2 to ptr), ptr %ptr4, align 8
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %types_header, i32 0, i32 0
  store i64 5, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %types_header, i32 0, i32 1
  store i64 5, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %types_header, i32 0, i32 2
  store ptr %types_data, ptr %2, align 8
  %df_ptr = call ptr inttoptr (i64 140727031823776 to ptr)(i64 24)
  %cols = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr, i32 0, i32 0
  %rows = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr, i32 0, i32 1
  %types = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr, i32 0, i32 2
  store ptr %names_header, ptr %cols, align 8
  store ptr %raw_rows_ptr, ptr %rows, align 8
  store ptr %types_header, ptr %types, align 8
  store ptr %df_ptr, ptr inttoptr (i64 2646027144800 to ptr), align 8
  %runtime_obj = call ptr inttoptr (i64 140727031823776 to ptr)(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}
